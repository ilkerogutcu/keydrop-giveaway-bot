using Figgle;
using KeyDropGiveawayBot.Services;
using KeyDropGiveawayBot.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.ObjectPool;
using Serilog;
using Serilog.Events;

#region Service Configuration

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();
var configuration = builder.Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Error)
    .MinimumLevel.Override("System", LogEventLevel.Error)
    .WriteTo.Console(outputTemplate: "[{Timestamp:y-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

using var host = Host.CreateDefaultBuilder()
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddSerilog();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IApiClient, ApiClient>();
        services.AddSingleton<IKeyDropService, KeyDropService>();
        services.AddHttpClient();
        services.AddSingleton(context.Configuration);
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

        services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            return provider.Create(new StringBuilderPooledObjectPolicy());
        });
    })
    .ConfigureAppConfiguration((context, config) => { config.AddJsonFile("appsettings.json", false, true); })
    .Build();

#endregion


DisplayWelcomeMessage();

Log.Information("Starting KeyDrop Giveaway Bot");

var keyDropService = host.Services.GetRequiredService<IKeyDropService>();

while (true)
{
    var giveaways = await keyDropService.GetGiveawaysAsync();
    if (giveaways == null)
    {
        Log.Warning("Trying to get giveaways again in 10 seconds.");
        Thread.Sleep(10000);
        continue;
    }

    Parallel.ForEach(giveaways, async giveaway =>
    {
        var giveawayDetails = await keyDropService.GetGiveawayDetailsByIdAsync(giveaway.Id);
        if (giveawayDetails is null)
        {
            Log.Warning("An error occurred while getting giveaway details.");
            return;
        }

        if (giveawayDetails.HaveIJoined == true)
        {
            Log.Information($"Already joined giveaway {giveawayDetails.Id}. Currently in {giveawayDetails.ParticipantCount} users.");
            return;
        }

        switch (giveawayDetails.CanIJoin)
        {
            case false when giveawayDetails.Status != "ended":
                return;
            case true when giveawayDetails.Status != "ended":
            {
                Log.Information($"Joining giveaway {giveawayDetails.Id}.");
                await keyDropService.JoinGiveawayAsync(giveawayDetails.Id);
                return;
            }
            default:
                Log.Warning(
                    $"Giveaway {giveawayDetails.Id} is not joinable. Giveaway Status: {giveawayDetails.Status}.");
                break;
        }
    });
    await Task.Delay(10000);
}

void DisplayWelcomeMessage()
{
    Console.WriteLine(FiggleFonts.Standard.Render("KeyDrop Giveaway Bot"));
    Console.WriteLine(
        "This keydrop giveaway bot is developed by @ilkerogutcu. You can find the source code on GitHub: https://github.com/ilkerogutcu/keydrop-giveaway-bot");
    Console.WriteLine("If you have any questions, you can contact me on Discord: ilker#1828");
    Console.WriteLine("Luck to you!");
    Console.WriteLine();
}
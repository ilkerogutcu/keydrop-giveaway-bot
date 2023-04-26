using Figgle;
using KeyDropGiveawayBot.Models;
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
        services.AddSingleton<ISessionService, SessionService>();
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
var sessionService = host.Services.GetRequiredService<ISessionService>();

await sessionService.SetKeyDropCookieAsync();

var joinedGiveaways = new Dictionary<string, GiveawayStatus>();

var tasks = new List<Task>
{
    Task.Run(async () =>
    {
        while (true)
        {
            var unknownJoinedGiveaways = joinedGiveaways.Where(giveaway => giveaway.Value == GiveawayStatus.Unknown)
                .Select(giveaway => giveaway.Key).ToList();
            foreach (var unknownJoinedGiveaway in unknownJoinedGiveaways)
            {
                var giveawayDetails = await keyDropService.GetGiveawayDetailsByIdAsync(unknownJoinedGiveaway);
                if (giveawayDetails is null)
                {
                    Log.Warning("An error occurred while getting giveaway details");
                    continue;
                }

                if (giveawayDetails.Status != "ended") continue;

                var haveIWon = giveawayDetails.Winners
                    .Select(winner => winner.UserData.IdSteam == giveawayDetails.MySteamId).Any(haveIWon => haveIWon);

                if (!haveIWon)
                {
                    joinedGiveaways[unknownJoinedGiveaway] = GiveawayStatus.Lost;
                    continue;
                }

                Log.Information("You won the giveaway {GiveawayDetailsId}!", giveawayDetails.Id);
            }

            await Task.Delay(10000);
        }
    }),

    Task.Run(async () =>
    {
        while (true)
        {
            var giveaways = await keyDropService.GetGiveawaysAsync();
            if (giveaways == null)
            {
                Log.Warning("Trying to get giveaways again in 10 seconds");
                Thread.Sleep(10000);
                continue;
            }

            giveaways.Reverse();
            foreach (var giveaway in giveaways)
            {
                var giveawayDetails = await keyDropService.GetGiveawayDetailsByIdAsync(giveaway.Id);
                if (giveawayDetails is null)
                {
                    Log.Warning("An error occurred while getting giveaway details");
                    continue;
                }

                if (giveawayDetails.HaveIJoined == true)
                {
                    Log.Information(
                        "Already joined giveaway {GiveawayDetailsId}. Currently in {GiveawayDetailsParticipantCount} users",
                        giveawayDetails.Id, giveawayDetails.ParticipantCount);
                    continue;
                }

                switch (giveawayDetails.CanIJoin)
                {
                    case false when giveawayDetails.Status != "ended":
                        break;
                    case true when giveawayDetails.Status != "ended":
                    {
                        var joinGiveawayResponse = await keyDropService.JoinGiveawayAsync(giveawayDetails.Id);
                        if (joinGiveawayResponse is not null)
                        {
                            joinedGiveaways.Add(giveawayDetails.Id, GiveawayStatus.Unknown);
                        }

                        break;
                    }
                    default:
                        Log.Warning(
                            "Giveaway {GiveawayDetailsId} is not joinable. Giveaway Status: {GiveawayDetailsStatus}",
                            giveawayDetails.Id, giveawayDetails.Status);
                        break;
                }

                await Task.Delay(1000);
            }

            UpdateConsoleTitle();
            await Task.Delay(10000);
        }
    })
};

await Task.WhenAll(tasks);


void DisplayWelcomeMessage()
{
    Console.WriteLine(FiggleFonts.Standard.Render("KeyDrop Giveaway Bot"));
    Console.WriteLine(
        "This keydrop giveaway bot is developed by @ilkerogutcu. You can find the source code on GitHub: https://github.com/ilkerogutcu/keydrop-giveaway-bot");
    Console.WriteLine("If you have any questions, you can contact me on Discord: ilker#1828");
    Console.WriteLine("Luck to you!");
    Console.WriteLine();
}

void UpdateConsoleTitle()
{
    var totalGiveawayCount = joinedGiveaways.Count;
    var wonGiveawayCount = joinedGiveaways.Count(giveaway => giveaway.Value == GiveawayStatus.Won);
    var lostGiveawayCount = joinedGiveaways.Count(giveaway => giveaway.Value == GiveawayStatus.Lost);

    Console.Title =
        $"KeyDrop Giveaway Bot | Joined Giveaways: {totalGiveawayCount} | Won Giveaways: {wonGiveawayCount} | Lost Giveaways: {lostGiveawayCount}";
}
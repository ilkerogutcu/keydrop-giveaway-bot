using Figgle;
using KeyDropGiveawayBot.Services;
using KeyDropGiveawayBot.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ObjectPool;
using Serilog;

var builder = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddEnvironmentVariables();
var configuration = builder.Build();

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console(outputTemplate: "[{Timestamp:y-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File($"logs\\log-.txt", rollingInterval: RollingInterval.Day,
        outputTemplate: "[{Timestamp:y-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

using var host = Host.CreateDefaultBuilder()
    .ConfigureServices((context, services) =>
    {
        services.AddScoped<IApiClient, ApiClient>();
        services.AddSingleton<IKeyDropService,KeyDropService>();
        services.AddHttpClient();
        services.AddSingleton(context.Configuration);
        services.TryAddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider> ();

        services.TryAddSingleton(serviceProvider =>
        {
            var provider = serviceProvider.GetRequiredService<ObjectPoolProvider>();
            return provider.Create(new StringBuilderPooledObjectPolicy());
        });
    })
    .ConfigureAppConfiguration((context, config) => { config.AddJsonFile("appsettings.json", false, true); })
    .Build();


Console.WriteLine(FiggleFonts.Standard.Render("KeyDrop Giveaway Bot"));
Log.Information("Starting KeyDrop Giveaway Bot");


var giveawayList = await host.Services.GetRequiredService<IKeyDropService>().GetGiveawaysAsync();
Log.CloseAndFlush();
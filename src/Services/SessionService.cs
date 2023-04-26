using KeyDropGiveawayBot.Constants;
using KeyDropGiveawayBot.Models;
using KeyDropGiveawayBot.Utils;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium.Chrome;
using SeleniumUndetectedChromeDriver;
using Serilog;

namespace KeyDropGiveawayBot.Services;

public class SessionService : ISessionService
{
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _configuration;

    public SessionService(IApiClient apiClient, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _configuration = configuration;
    }

    public async Task SetKeyDropCookieAsync(CancellationToken cancellationToken = default)
    {
        Log.Information("Setting KeyDrop cookie...");
        var cloudFlareCookieValue = await GetCloudFlareCookieValueAsync();
        var vioShieldCookieValue = await GetVioShieldCookieValueAsync(cloudFlareCookieValue, cancellationToken);
        var cookie =
            $"__cf_bm={cloudFlareCookieValue}; __vioShield={vioShieldCookieValue}; session_id={_configuration["sessionId"]}; key-lang=TR";
        _configuration["cookie"] = cookie;
        Log.Information("KeyDrop cookie set successfully");
    }

    private async Task<string> GetVioShieldCookieValueAsync(string cfBm,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(_configuration["userAgent"]))
            throw new Exception("User agent is not set. Please check your configuration.");

        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>
        {
            { "authority", "key-drop.com" },
            { "accept", "*/*" },
            { "accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7" },
            { "cookie", $"session_id={_configuration["sessionId"]}; key-lang=TR; __cf_bm={cfBm}" },
            { "referer", "https://key-drop.com/tr/token" },
            { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "\"Windows\"" },
            { "user-agent", _configuration["userAgent"] }
        };

        var response = await _apiClient.AddHeaders(headers)
            .GetHttpResponseAsync<object>(KeyDropConstants.VioShieldEndpoint, HttpRequestType.Get, null,
                null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception(
                "An error occurred while getting vioShield cookie value. Please check your session id.");

        var vioShieldCookieValue = response.Headers.GetValues("set-cookie")
            .FirstOrDefault(x => x.Contains("__vioShield"))?.Split(";")[0].Split("=")[1];

        if (string.IsNullOrEmpty(vioShieldCookieValue))
            throw new Exception(
                "An error occurred while getting vioShield cookie value. Please check your session id.");

        return vioShieldCookieValue;
    }

    private static async Task<string> GetCloudFlareCookieValueAsync()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("--disable-notifications");
        using var driver = UndetectedChromeDriver.Create(
            driverExecutablePath: await new ChromeDriverInstaller().Auto(), options: chromeOptions, headless: true,
            configureService:
            opt =>
            {
                opt.SuppressInitialDiagnosticInformation = true;
                opt.HideCommandPromptWindow = true;
            });

        driver.GoToUrl(KeyDropConstants.KeyDropEndpoint);

        var cfBm = driver.Manage().Cookies?.GetCookieNamed("__cf_bm")?.Value;

        if (string.IsNullOrEmpty(cfBm))
            throw new Exception(
                "An error occurred while getting cloudflare cookie value. Please check your session id.");

        driver.Close();
        driver.Quit();

        return cfBm;
    }
}
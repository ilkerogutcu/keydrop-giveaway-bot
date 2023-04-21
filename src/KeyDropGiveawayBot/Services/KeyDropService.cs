using KeyDropGiveawayBot.Constants;
using KeyDropGiveawayBot.Models;
using KeyDropGiveawayBot.Utils;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace KeyDropGiveawayBot.Services;

public class KeyDropService : IKeyDropService
{
    private readonly IApiClient _apiClient;
    private readonly IConfiguration _configuration;

    public KeyDropService(IApiClient apiClient, IConfiguration configuration)
    {
        _apiClient = apiClient;
        _configuration = configuration;
    }

    public async Task<List<Giveaway>> GetGiveawaysAsync()
    {
        try
        {
            var configurationIsNullOrEmpty = ConfigurationIsNullOrEmpty();
            if (configurationIsNullOrEmpty)
            {
                return new List<Giveaway>();
            }

            var headers = new Dictionary<string, string>();
            headers.Add("authority", "wss-2061.key-drop.com");
            headers.Add("accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            headers.Add("authorization", $"Bearer {_configuration["Token"]}");
            headers.Add("accept-language", "tr-TR,tr;q=0.9");
            headers.Add("cache-control", "max-age=0");
            headers.Add("sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
            headers.Add("sec-ch-ua-mobile", "?0");
            headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers.Add("sec-fetch-dest", "document");
            headers.Add("sec-fetch-mode", "navigate");
            headers.Add("sec-fetch-site", "none");
            headers.Add("sec-fetch-user", "?1");
            headers.Add("upgrade-insecure-requests", "1");
            headers.Add("user-agent", _configuration["UserAgent"]);
            headers.Add("Cookie", _configuration["Cookie"]);

            var response = await _apiClient.AddHeaders(headers)
                .GetAsync<PaginatedKeyDropResponse<Giveaway>>(KeyDropEndpoints.GetGiveawayListEndpoint);
            return response.Data;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting giveaways.");
            return new List<Giveaway>();
        }
    }

    public async Task<GiveawayResult?> GetGiveawayResultByIdAsync(string giveawayId)
    {
        var configurationIsValid = ConfigurationIsNullOrEmpty();
        if (configurationIsValid)
        {
            return null;
        }

        var headers = new Dictionary<string, string>();
        headers.Add("authority", "wss-2061.key-drop.com");
        headers.Add("accept", "*/*");
        headers.Add("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
        headers.Add("authorization", $"Bearer {_configuration["Token"]}");
        headers.Add("origin", "https://key-drop.com");
        headers.Add("referer", "https://key-drop.com/");
        headers.Add("sec-ch-ua",
            "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
        headers.Add("sec-ch-ua-mobile", "?0");
        headers.Add("sec-ch-ua-platform", "\"Windows\"");
        headers.Add("sec-fetch-dest", "empty");
        headers.Add("sec-fetch-mode", "cors");
        headers.Add("sec-fetch-site", "same-site");
        headers.Add("user-agent", _configuration["UserAgent"]);
        headers.Add("x-currency", "USD");
        headers.Add("cookie", _configuration["Cookie"]);


        var response = await _apiClient.AddHeaders(headers).GetAsync<BaseKeyDropResponse<GiveawayResult>>(
            string.Format(KeyDropEndpoints.GetGiveawayResultEndpoint, giveawayId));
        return response.Data;
    }

    private bool ConfigurationIsNullOrEmpty()
    {
        if (string.IsNullOrEmpty(_configuration["Token"]))
        {
            Log.Error("Token is not set in configuration. Please check your configuration file.");
            return true;
        }

        if (string.IsNullOrEmpty(_configuration["Cookie"]))
        {
            Log.Error("Cookie is not set in configuration. Please check your configuration file.");
            return true;
        }

        if (string.IsNullOrEmpty(_configuration["UserAgent"]))
        {
            Log.Error("UserAgent is not set in configuration. Please check your configuration file.");
            return true;
        }

        return false;
    }
}
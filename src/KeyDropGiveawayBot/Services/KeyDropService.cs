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
            var configurationIsValid = CheckConfiguration();
            if (configurationIsValid is false)
            {
                return new List<Giveaway>();
            }

            var headers = new Dictionary<string, string>
            {
                { "authority", "wss-2061.key-drop.com" },
                {
                    "accept",
                    "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
                },
                { "authorization", $"Bearer {_configuration["Token"]}" },
                { "accept-language", "tr-TR,tr;q=0.9" },
                { "cache-control", "max-age=0" },
                { "if-none-match", "W/\"c8d-7YKgCQSfv2Rrlm1cLA4sKiacpKE\"" },
                { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
                { "sec-ch-ua-mobile", "?0" },
                { "sec-ch-ua-platform", "\"Windows\"" },
                { "sec-fetch-dest", "document" },
                { "sec-fetch-mode", "navigate" },
                { "sec-fetch-site", "none" },
                { "sec-fetch-user", "?1" },
                { "upgrade-insecure-requests", "1" },
                { "user-agent", _configuration["UserAgent"] },
                { "Cookie", _configuration["Cookie"] }
            };

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

    private bool CheckConfiguration()
    {
        if (string.IsNullOrEmpty(_configuration["Token"]))
        {
            Log.Error("Token is not set in configuration. Please check your configuration file.");
            return false;
        }

        if (string.IsNullOrEmpty(_configuration["Cookie"]))
        {
            Log.Error("Cookie is not set in configuration. Please check your configuration file.");
            return false;
        }

        if (string.IsNullOrEmpty(_configuration["UserAgent"]))
        {
            Log.Error("UserAgent is not set in configuration. Please check your configuration file.");
            return false;
        }

        return true;
    }
}
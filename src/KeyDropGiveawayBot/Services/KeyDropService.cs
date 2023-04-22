using KeyDropGiveawayBot.Constants;
using KeyDropGiveawayBot.Exceptions;
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

    public async Task<List<Giveaway>?> GetGiveawaysAsync()
    {
        try
        {
            var configurationIsNullOrEmpty = ConfigurationIsNullOrEmpty();
            if (configurationIsNullOrEmpty)
            {
                return null;
            }

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var headers = new Dictionary<string, string>();
            headers.Add("authority", "wss-2061.key-drop.com");
            headers.Add("accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            headers.Add("authorization", $"Bearer {token}");
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
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting giveaways.");
            return null;
        }
    }

    public async Task<GiveawayDetail?> GetGiveawayDetailsByIdAsync(string giveawayId)
    {
        try
        {
            var configurationIsValid = ConfigurationIsNullOrEmpty();
            if (configurationIsValid)
            {
                return null;
            }

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var headers = new Dictionary<string, string>();
            headers.Add("authority", "wss-2061.key-drop.com");
            headers.Add("accept", "*/*");
            headers.Add("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("authorization", $"Bearer {token}");
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


            var response = await _apiClient.AddHeaders(headers).GetAsync<BaseKeyDropResponse<GiveawayDetail>>(
                string.Format(KeyDropEndpoints.GetGiveawayResultEndpoint, giveawayId));
            return response.Data;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting giveaway result.");
            return null;
        }
    }

    public async Task<JoinGiveawayResponse?> JoinGiveawayAsync(string giveawayId)
    {
        try
        {
            var configurationIsValid = ConfigurationIsNullOrEmpty();
            if (configurationIsValid)
            {
                return null;
            }

            var token = await GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return null;
            }

            var headers = new Dictionary<string, string>();
            headers.Add("authority", "wss-3002.key-drop.com");
            headers.Add("accept", "*/*");
            headers.Add("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("authorization", $"Bearer {token}");
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
            headers.Add("cookie", _configuration["Cookie"]);

            var response = await _apiClient.AddHeaders(headers)
                .PutAsync<object, BaseKeyDropResponse<JoinGiveawayResponse>>(
                    string.Format(KeyDropEndpoints.JoinGiveawayEndpoint, giveawayId), null);

            if (!response.Success)
            {
                if (!string.IsNullOrEmpty(response.Message) && response.Message.Equals("captcha"))
                {
                    Log.Error("Captcha is required. Please solve captcha and join giveaway on browser.");
                    return null;
                }

                Log.Error($"An error occurred while joining giveaway {giveawayId}. Error: {response.Message}");
                return null;
            }

            Log.Information($"Joined successfully giveaway {giveawayId}.");
            return response.Data;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while joining giveaway.");
            return null;
        }
    }

    private bool ConfigurationIsNullOrEmpty()
    {
        if (string.IsNullOrEmpty(_configuration["cookie"]))
        {
            Log.Error("Cookie is not set in configuration. Please check your configuration file.");
            return true;
        }

        if (string.IsNullOrEmpty(_configuration["userAgent"]))
        {
            Log.Error("UserAgent is not set in configuration. Please check your configuration file.");
            return true;
        }

        return false;
    }

    private async Task<string> GetTokenAsync()
    {
        try
        {
            var headers = new Dictionary<string, string>();
            headers.Add("authority", "key-drop.com");
            headers.Add("accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            headers.Add("accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7");
            headers.Add("cache-control", "max-age=0");
            headers.Add("origin", "https://key-drop.com");
            headers.Add("referer", "https://key-drop.com/");
            headers.Add("sec-ch-ua",
                "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"");
            headers.Add("sec-ch-ua-mobile", "?0");
            headers.Add("sec-ch-ua-platform", "\"Windows\"");
            headers.Add("sec-fetch-dest", "document");
            headers.Add("sec-fetch-mode", "navigate");
            headers.Add("sec-fetch-site", "none");
            headers.Add("sec-fetch-user", "?1");
            headers.Add("upgrade-insecure-requests", "1");
            headers.Add("user-agent", _configuration["userAgent"]);
            headers.Add("cookie", _configuration["cookie"]);
            var response = await _apiClient.AddHeaders(headers)
                .GetResponseContentAsStringAsync(KeyDropEndpoints.GetTokenEndpoint);
            return response;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting token.");
            return string.Empty;
        }
    }
}
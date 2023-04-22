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
        var configurationIsNullOrEmpty = KeyDropConfigurationIsNullOrEmpty();
        if (configurationIsNullOrEmpty)
        {
            return null;
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>
        {
            { "authority", "wss-2061.key-drop.com" },
            {
                "accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
            },
            { "authorization", $"Bearer {token}" },
            { "accept-language", "tr-TR,tr;q=0.9" },
            { "cache-control", "max-age=0" },
            { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "\"Windows\"" },
            { "sec-fetch-dest", "document" },
            { "sec-fetch-mode", "navigate" },
            { "sec-fetch-site", "none" },
            { "sec-fetch-user", "?1" },
            { "upgrade-insecure-requests", "1" },
            { "user-agent", _configuration["userAgent"] },
            { "Cookie", _configuration["cookie"] }
        };
        try
        {
            var response = await _apiClient.AddHeaders(headers)
                .GetAsync<PaginatedKeyDropResponse<Giveaway>>(KeyDropEndpoints.GetGiveawayListEndpoint);
            return response.Data;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (TaskCanceledException)
        {
            Log.Error("Request time out while getting giveaways. Server is not responding");
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting giveaways.");
            return null;
        }
    }

    public async Task<GiveawayDetails?> GetGiveawayDetailsByIdAsync(string giveawayId)
    {
        var configurationIsNullOrEmpty = KeyDropConfigurationIsNullOrEmpty();
        if (configurationIsNullOrEmpty)
        {
            return null;
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>
        {
            { "authority", "wss-2061.key-drop.com" },
            { "accept", "*/*" },
            { "accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7" },
            { "authorization", $"Bearer {token}" },
            { "origin", "https://key-drop.com" },
            { "referer", "https://key-drop.com/" },
            { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "\"Windows\"" },
            { "sec-fetch-dest", "empty" },
            { "sec-fetch-mode", "cors" },
            { "sec-fetch-site", "same-site" },
            { "user-agent", _configuration["userAgent"] },
            { "x-currency", "USD" },
            { "cookie", _configuration["cookie"] }
        };

        try
        {
            var response = await _apiClient.AddHeaders(headers).GetAsync<BaseKeyDropResponse<GiveawayDetails>>(
                string.Format(KeyDropEndpoints.GetGiveawayResultEndpoint, giveawayId));
            return response.Data;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (TaskCanceledException)
        {
            Log.Error("Request time out while getting giveaway result. Server not responding");
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
        var configurationIsNullOrEmpty = KeyDropConfigurationIsNullOrEmpty();
        if (configurationIsNullOrEmpty)
        {
            return null;
        }

        var token = await GetTokenAsync();
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>
        {
            { "authority", "wss-3002.key-drop.com" },
            { "accept", "*/*" },
            { "accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7" },
            { "authorization", $"Bearer {token}" },
            { "origin", "https://key-drop.com" },
            { "referer", "https://key-drop.com/" },
            { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "\"Windows\"" },
            { "sec-fetch-dest", "empty" },
            { "sec-fetch-mode", "cors" },
            { "sec-fetch-site", "same-site" },
            { "user-agent", _configuration["userAgent"] },
            { "cookie", _configuration["cookie"] }
        };

        try
        {
            var cts = new CancellationTokenSource();

            var response = await _apiClient.AddHeaders(headers)
                .PutAsync<object, BaseKeyDropResponse<JoinGiveawayResponse>>(
                    string.Format(KeyDropEndpoints.JoinGiveawayEndpoint, giveawayId), null,
                    cancellationToken: cts.Token);

            if (!response.Success)
            {
                if (!string.IsNullOrEmpty(response.Message) && response.Message.Equals("captcha"))
                {
                    Log.Error("Captcha is required. Please solve captcha and join giveaway on browser");
                    return null;
                }

                Log.Error("An error occurred while joining giveaway {GiveawayId}. Error: {ResponseMessage}", giveawayId, response.Message);
                return null;
            }

            Log.Information("Joined successfully giveaway {GiveawayId}", giveawayId);
            return response.Data;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (TaskCanceledException)
        {
            Log.Error("Join giveaway request is timed out. Server is not responding");
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while joining giveaway");
            return null;
        }
    }

    private async Task<string?> GetTokenAsync()
    {
        var configurationIsNullOrEmpty = KeyDropConfigurationIsNullOrEmpty();
        if (configurationIsNullOrEmpty)
        {
            return null;
        }

        IReadOnlyDictionary<string, string> headers = new Dictionary<string, string>
        {
            { "authority", "key-drop.com" },
            {
                "accept",
                "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7"
            },
            { "accept-language", "tr-TR,tr;q=0.9,en-US;q=0.8,en;q=0.7" },
            { "cache-control", "max-age=0" },
            { "origin", "https://key-drop.com" },
            { "referer", "https://key-drop.com/" },
            { "sec-ch-ua", "\"Chromium\";v=\"112\", \"Google Chrome\";v=\"112\", \"Not:A-Brand\";v=\"99\"" },
            { "sec-ch-ua-mobile", "?0" },
            { "sec-ch-ua-platform", "\"Windows\"" },
            { "sec-fetch-dest", "document" },
            { "sec-fetch-mode", "navigate" },
            { "sec-fetch-site", "none" },
            { "sec-fetch-user", "?1" },
            { "upgrade-insecure-requests", "1" },
            { "user-agent", _configuration["userAgent"] },
            { "cookie", _configuration["cookie"] }
        };

        var cts = new CancellationTokenSource();

        try
        {
            var response = await _apiClient.AddHeaders(headers)
                .GetResponseContentAsStringAsync(KeyDropEndpoints.GetTokenEndpoint,
                    cancellationToken: cts.Token);
            return response;
        }
        catch (ExpiredCookieException e)
        {
            Log.Error(e.Message);
            return null;
        }
        catch (TaskCanceledException e)
        {
            Log.Error("Request timed out while getting token. Server is not responding");
            return null;
        }
        catch (Exception e)
        {
            Log.Error(e, "An error occurred while getting token");
            return null;
        }
    }

    private bool KeyDropConfigurationIsNullOrEmpty()
    {
        if (string.IsNullOrEmpty(_configuration["cookie"]))
        {
            Log.Error("Cookie is not set in configuration. Please check your configuration file");
            return true;
        }

        if (string.IsNullOrEmpty(_configuration["userAgent"]))
        {
            Log.Error("UserAgent is not set in configuration. Please check your configuration file");
            return true;
        }

        return false;
    }
}
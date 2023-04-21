using System.Net;
using System.Text;
using KeyDropGiveawayBot.Constants;
using KeyDropGiveawayBot.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ObjectPool;
using Newtonsoft.Json;

namespace KeyDropGiveawayBot.Utils;

public class ApiClient : IApiClient
{
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ObjectPool<StringBuilder> _builderPool;
    private IReadOnlyDictionary<string, string> _headers = new Dictionary<string, string>();

    public ApiClient(IHttpClientFactory httpClientFactory, ObjectPool<StringBuilder> builderPool,
        IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _builderPool = builderPool;
        _configuration = configuration;
    }

    public IApiClient AddHeaders(IReadOnlyDictionary<string, string> headers)
    {
        _headers = headers;
        return this;
    }

    public async Task<T> GetAsync<T>(string url, IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null)
        where T : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse = await httpClient.GetAsync(uri);

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<T>(httpClientResponse);
        return response;
    }

    public async Task<TOut> PostAsync<TIn, TOut>(string url, TIn payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null) where TIn : class where TOut : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse =
            await httpClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(payload)));

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<TOut>(httpClientResponse);
        return response;
    }

    public async Task<TOut> PutAsync<TIn, TOut>(string url, TIn payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null) where TIn : class where TOut : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse =
            await httpClient.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(payload)));

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<TOut>(httpClientResponse);
        return response;
    }

    private static async Task<TOut> GetResponseAsync<TOut>(HttpResponseMessage httpClientResponse) where TOut : class
    {
        var content = await httpClientResponse.Content.ReadAsStringAsync();
        var response = JsonConvert.DeserializeObject<TOut>(content);
        if (response == null)
            throw new Exception($"An error occurred while deserializing {content}");

        return response;
    }

    private string GetUri(string url, string? queryParameter)
    {
        var queryParameterIsEmpty = string.IsNullOrEmpty(queryParameter);
        var startsWithQuestionMark = queryParameter?.StartsWith(ApiClientConstants.QuestionMark) ?? false;

        if (!queryParameterIsEmpty && !startsWithQuestionMark)
            throw new ArgumentException("Query parameter must start with question mark", nameof(queryParameter));

        return $"{url}{queryParameter}";
    }

    private HttpClient GetHttpClient()
    {
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(_configuration.GetValue<int>("requestTimeout"));
        return httpClient;
    }

    private string GetQueryString(IReadOnlyList<KeyValuePair<string, object>>? queryParameters)
    {
        if (queryParameters == null || queryParameters.Count == 0)
            return string.Empty;

        var builder = _builderPool.Get();
        builder.Append(ApiClientConstants.QuestionMark);

        foreach (var (key, value) in queryParameters)
        {
            builder.Append(key);
            builder.Append(ApiClientConstants.Equal);
            builder.Append(value);
            builder.Append(ApiClientConstants.And);
        }

        var queryString = builder.RemoveLast();
        _builderPool.Return(builder);
        return queryString;
    }

    private void AddHeaders(HttpClient client)
    {
        foreach (var (key, value) in _headers)
        {
            if (client.DefaultRequestHeaders.Contains(key))
            {
                client.DefaultRequestHeaders.Remove(key);
            }

            client.DefaultRequestHeaders.Add(key, value);
        }
    }
}
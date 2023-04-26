using System.Net;
using System.Text;
using KeyDropGiveawayBot.Constants;
using KeyDropGiveawayBot.Exceptions;
using KeyDropGiveawayBot.Extensions;
using KeyDropGiveawayBot.Models;
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

    public async Task<T> GetAsync<T>(string url, IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null,
        CancellationToken cancellationToken = default)
        where T : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse = await httpClient.GetAsync(uri, cancellationToken);

        if (httpClientResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new ExpiredCookieException();

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<T>(httpClientResponse, cancellationToken);
        return response;
    }

    public async Task<string> GetResponseContentAsStringAsync(string url,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse = await httpClient.GetAsync(uri, cancellationToken);
        var response = await httpClientResponse.Content.ReadAsStringAsync(cancellationToken);

        if (httpClientResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new ExpiredCookieException();

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        return response;
    }

    public async Task<HttpResponseMessage> GetHttpResponseAsync<T>(string url, HttpRequestType requestType, T? payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null,
        CancellationToken cancellationToken = default)
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);

        AddHeaders(httpClient);

        var httpClientResponse = requestType switch
        {
            HttpRequestType.Get => await httpClient.GetAsync(uri, cancellationToken),
            HttpRequestType.Post => await httpClient.PostAsync(uri,
                new StringContent(JsonConvert.SerializeObject(payload)), cancellationToken),
            HttpRequestType.Put => await httpClient.PutAsync(uri,
                new StringContent(JsonConvert.SerializeObject(payload)), cancellationToken),
            HttpRequestType.Delete => await httpClient.DeleteAsync(uri, cancellationToken),
            HttpRequestType.Patch => await httpClient.PatchAsync(uri,
                new StringContent(JsonConvert.SerializeObject(payload)), cancellationToken),
            _ => throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null)
        };

        return httpClientResponse;
    }

    public async Task<TOut> PostAsync<TIn, TOut>(string url, TIn payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null,
        CancellationToken cancellationToken = default) where TIn : class where TOut : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        var httpClientResponse =
            await httpClient.PostAsync(uri, new StringContent(JsonConvert.SerializeObject(payload)), cancellationToken);

        if (httpClientResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new ExpiredCookieException();

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<TOut>(httpClientResponse, cancellationToken);
        return response;
    }

    public async Task<TOut> PutAsync<TIn, TOut>(string url, TIn? payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null,
        CancellationToken cancellationToken = default) where TIn : class where TOut : class
    {
        var uri = GetUri(url, GetQueryString(queryParameters));
        var httpClient = GetHttpClient();
        httpClient.BaseAddress = new Uri(uri);
        AddHeaders(httpClient);

        HttpResponseMessage httpClientResponse;

        if (payload is null)
        {
            httpClientResponse = await httpClient.PutAsync(uri, null, cancellationToken);
        }
        else
        {
            httpClientResponse = await httpClient.PutAsync(uri, new StringContent(JsonConvert.SerializeObject(payload)),
                cancellationToken);
        }

        if (httpClientResponse.StatusCode == HttpStatusCode.Forbidden)
            throw new ExpiredCookieException();

        if (httpClientResponse.StatusCode != HttpStatusCode.OK)
            throw new Exception(
                $"An error occurred while calling {uri} with status code {httpClientResponse.StatusCode}");

        var response = await GetResponseAsync<TOut>(httpClientResponse, cancellationToken);
        return response;
    }

    private static async Task<TOut> GetResponseAsync<TOut>(HttpResponseMessage httpClientResponse,
        CancellationToken cancellationToken = default) where TOut : class
    {
        var content = await httpClientResponse.Content.ReadAsStringAsync(cancellationToken);
        var response = JsonConvert.DeserializeObject<TOut>(content);
        if (response == null)
            throw new Exception($"An error occurred while deserializing {content}");

        return response;
    }

    private static string GetUri(string url, string? queryParameter)
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

    private string GetQueryString(IReadOnlyCollection<KeyValuePair<string, object>>? queryParameters)
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
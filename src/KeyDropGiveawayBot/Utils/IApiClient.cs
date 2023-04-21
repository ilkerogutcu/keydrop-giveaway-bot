namespace KeyDropGiveawayBot.Utils;

public interface IApiClient
{
    IApiClient AddHeaders(IReadOnlyDictionary<string, string> headers);

    Task<T> GetAsync<T>(string url, IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null)
        where T : class;

    Task<TOut> PostAsync<TIn, TOut>(string url, TIn payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null)
        where TIn : class
        where TOut : class;

    Task<TOut> PutAsync<TIn, TOut>(string url, TIn payload,
        IReadOnlyList<KeyValuePair<string, object>>? queryParameters = null)
        where TIn : class
        where TOut : class;
}
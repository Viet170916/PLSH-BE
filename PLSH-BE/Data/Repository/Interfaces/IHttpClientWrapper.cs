using System.Collections;

namespace Data.Repository.Interfaces
{
    public interface IHttpClientWrapper
    {
        Task<T> GetAsync<T>(string requestBaseUrl, string requestUrl);

        Task<T> PostAsync<T>(string requestBaseUrl, string requestUrl, object body);

        Task<T> PostMultiAsync<T>(string requestBaseUrl, string requestUrl, IList body);
    }
}

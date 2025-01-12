using System.Collections;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Data.Repository.Interfaces;
using Common.Library;
using Newtonsoft.Json;

namespace Data.Repository.Implementation
{
    [ExcludeFromCodeCoverage]
    public class HttpClientWrapper : IHttpClientWrapper
    {
        public async Task<T> GetAsync<T>(string requestBaseUrl, string requestUrl)
        {
            HttpClientHandler handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };
            using var client = new HttpClient(handler) { BaseAddress = new Uri(requestBaseUrl) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ApplicationJson));
            var result = await client.GetAsync(requestUrl).ConfigureAwait(false);
            result.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<T> PostAsync<T>(string requestBaseUrl, string requestUrl, object body)
        {
            var handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };

            using var client = new HttpClient(handler);
            var json = JsonConvert.SerializeObject(body);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            client.BaseAddress = new Uri(requestBaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ApplicationJson));
            var result = await client.PostAsync(requestUrl, stringContent).ConfigureAwait(false);

            result.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
        }

        public async Task<T> PostMultiAsync<T>(string requestBaseUrl, string requestUrl, IList body)
        {
            var subjects = body;
            var handler = new HttpClientHandler()
            {
                UseDefaultCredentials = true,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                UseProxy = false,
                ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true
            };

            using var client = new HttpClient(handler);
            var json = JsonConvert.SerializeObject(subjects);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
            client.BaseAddress = new Uri(requestBaseUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(Constants.ApplicationJson));
            var result = await client.PostAsync(requestUrl, stringContent).ConfigureAwait(false);
         
            result.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<T>(await result.Content.ReadAsStringAsync().ConfigureAwait(false));
        }
    }
}

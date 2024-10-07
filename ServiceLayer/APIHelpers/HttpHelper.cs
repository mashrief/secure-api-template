using Newtonsoft.Json;
using System.Text;

namespace ServiceLayer.APIHelpers
{
    public static class HttpHelper
    {
        private static readonly HttpClient _client = new();

        #region GetAsync

        public static async Task<T?> GetAsync<T>(string endpoint, Dictionary<string, string> headers, Dictionary<string, string>? queryParams = null)
        {
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                endpoint = $"{endpoint}?{query}";
            }

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            AddHeaders(request, headers);

            return await SendRequestAsync<T>(request);
        }

        #endregion

        #region PostAsync

        public static async Task<T?> PostAsync<T>(string endpoint, Dictionary<string, string> headers, ContentType contentType, PostBody postBody)
        {
            var request = CreateRequest(HttpMethod.Post, endpoint, headers, contentType, postBody);

            return await SendRequestAsync<T>(request);
        }

        public static async Task PostAsync(string endpoint, Dictionary<string, string> headers, ContentType contentType, PostBody postBody)
        {
            var request = CreateRequest(HttpMethod.Post, endpoint, headers, contentType, postBody);

            await SendRequestAsync(request);
        }

        #endregion

        #region PutAsync

        public static async Task<T?> PutAsync<T>(string endpoint, Dictionary<string, string> headers, ContentType contentType, PostBody putBody)
        {
            var request = CreateRequest(HttpMethod.Put, endpoint, headers, contentType, putBody);

            return await SendRequestAsync<T>(request);
        }

        public static async Task PutAsync(string endpoint, Dictionary<string, string> headers, ContentType contentType, PostBody putBody)
        {
            var request = CreateRequest(HttpMethod.Put, endpoint, headers, contentType, putBody);

            await SendRequestAsync(request);
        }

        #endregion

        #region DeleteAsync

        public static async Task<T?> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers, Dictionary<string, string>? queryParams = null)
        {
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                endpoint = $"{endpoint}?{query}";
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            AddHeaders(request, headers);

            return await SendRequestAsync<T>(request);
        }

        public static async Task DeleteAsync(string endpoint, Dictionary<string, string> headers, Dictionary<string, string>? queryParams = null)
        {
            if (queryParams?.Count > 0)
            {
                var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value)}"));
                endpoint = $"{endpoint}?{query}";
            }

            var request = new HttpRequestMessage(HttpMethod.Delete, endpoint);
            AddHeaders(request, headers);

            await SendRequestAsync(request);
        }

        #endregion

        #region Private Methods

        private static HttpRequestMessage CreateRequest(HttpMethod method, string endpoint, Dictionary<string, string> headers, ContentType contentType, PostBody body)
        {
            var request = new HttpRequestMessage(method, endpoint);

            AddHeaders(request, headers);
            AddContent(request, contentType, body);

            return request;
        }

        private static void AddHeaders(HttpRequestMessage request, Dictionary<string, string> headers)
        {
            if (headers == null) return;

            foreach (var header in headers)
            {
                request.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        private static void AddContent(HttpRequestMessage request, ContentType contentType, PostBody body)
        {
            if (body == null) return;

            request.Content = contentType switch
            {
                ContentType.UrlEncoded => new FormUrlEncodedContent(body.FormData ?? []),
                ContentType.Json => new StringContent(body.Json ?? string.Empty, Encoding.UTF8, "application/json"),
                _ => request.Content
            };
        }

        private static async Task<T?> SendRequestAsync<T>(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(responseContent);
        }

        private static async Task SendRequestAsync(HttpRequestMessage request)
        {
            var response = await _client.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        #endregion
    }

    #region PostBody

    public class PostBody
    {
        public string? Json { get; set; }
        public Dictionary<string, string>? FormData { get; set; }
    }

    #endregion

    #region ContentType

    public enum ContentType
    {
        Json,
        UrlEncoded
    }

    #endregion

}

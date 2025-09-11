using AbstUI.Resources;
using System.Net;
using System.Net.Http.Headers;
using static System.Net.WebRequestMethods;

namespace AbstUI.Blazor.Resources
{
    public class BlazorResourceManager : AbstResourceManager
    {
        private readonly HttpClient _httpClient;

        public BlazorResourceManager(HttpClient  httpClient)
        {
            _httpClient = httpClient;
        }

        public override string? ReadTextFile(string fileName)
        {
            try
            {
                var content = _httpClient.GetStringAsync(fileName).GetAwaiter().GetResult();
                return content;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override bool FileExists(string fileName)
        {
            // Try HEAD first (cheap if the server supports it)
            try
            {
                using var head = new HttpRequestMessage(HttpMethod.Head, fileName);
                using var resp = _httpClient.SendAsync(head, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
                if (resp.StatusCode == HttpStatusCode.NotFound) return false;
                if (resp.IsSuccessStatusCode) return true;
                // fall through to GET on 405/other statuses
            }
            catch
            {
                // fall back to GET
            }

            // Fallback: GET headers / first byte only
            using var get = new HttpRequestMessage(HttpMethod.Get, fileName);
            get.Headers.Range = new RangeHeaderValue(0, 0); // request 1 byte if supported
            using var resp2 = _httpClient.SendAsync(get, HttpCompletionOption.ResponseHeadersRead).GetAwaiter().GetResult();
            return resp2.IsSuccessStatusCode;
        }
        public override byte[]? ReadBytes(string fileName)
        {
            return _httpClient.GetByteArrayAsync(fileName).GetAwaiter().GetResult();
        }
        
    }
}

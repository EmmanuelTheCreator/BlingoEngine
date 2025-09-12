using AbstUI.Resources;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace AbstUI.Blazor.Resources
{
    public class BlazorResourceManager : AbstResourceManager
    {
        private readonly HttpClient _httpClient;

        public BlazorResourceManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public override async Task<string?> ReadTextFileAsync(string fileName)
        {
            try
            {
                var content = await _httpClient.GetStringAsync(fileName);
                return content;
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override string? ReadTextFile(string fileName) => ReadTextFileAsync(fileName).GetAwaiter().GetResult();

        public override async Task<bool> FileExistsAsync(string fileName)
        {
            // Try HEAD first (cheap if the server supports it)
            try
            {
                using var head = new HttpRequestMessage(HttpMethod.Head, fileName);
                using var resp = await _httpClient.SendAsync(head, HttpCompletionOption.ResponseHeadersRead);
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
            using var resp2 = await _httpClient.SendAsync(get, HttpCompletionOption.ResponseHeadersRead);
            return resp2.IsSuccessStatusCode;
        }

        public override bool FileExists(string fileName) => FileExistsAsync(fileName).GetAwaiter().GetResult();

        public override async Task<byte[]?> ReadBytesAsync(string fileName)
        {
            try
            {
                return await _httpClient.GetByteArrayAsync(fileName);
            }
            catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public override byte[]? ReadBytes(string fileName) => ReadBytesAsync(fileName).GetAwaiter().GetResult();

    }
}

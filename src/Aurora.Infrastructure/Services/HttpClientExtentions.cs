using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Infrastructure.Services
{
    public static class HttpClientExtentions
    {
        public static async Task<string> GetStringAsync(this HttpClient client, string url)
        {
            var response = await MakeRequestAsync(client, url);

            if (!response.IsSuccessStatusCode)
            {
                return "";
            }

            return await response.Content.ReadFromJsonAsync<string>();
        }

        public static async Task<HttpResponseMessage> MakeRequestAsync(this HttpClient client, string url)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            var response = await client.SendAsync(request);
            return response;
        }
    }
}

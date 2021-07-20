using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace QckMox.Demo.Simple.Http
{
    public static class HttpHelper
    {
        public static async Task<T> SendHttpRequest<T>(IHttpClientFactory clientFactory, HttpRequestMessage request)
        {
            using (var client = clientFactory.CreateClient())
            {
                using (var responseMessage = await client.SendAsync(request))
                {
                    responseMessage.EnsureSuccessStatusCode();
                    var content = await responseMessage.Content.ReadAsStringAsync();
                    var response = JsonConvert.DeserializeObject<T>(content);
                    return response;
                }
            }
        }
    }
}
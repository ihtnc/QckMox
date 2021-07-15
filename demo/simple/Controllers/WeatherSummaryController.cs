using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QckMox.Demo.Simple.Http;
using QckMox.Demo.Simple.Models;

namespace QckMox.Demo.Simple.Controllers
{
    [ApiController]
    [Route("api/weathersummary")]
    public class WeatherSummaryController : ControllerBase
    {
        [HttpGet]
        public async Task<WeatherSummary> Get(int id, [FromServices] IHttpClientFactory clientFactory)
        {
            var pathBase = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
            var baseUri = new Uri(pathBase);
            var requestUri = new Uri(baseUri, $"/api/qckmox/summary/{id}");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await HttpHelper.SendHttpRequest<ApiResponse<WeatherSummary>>(clientFactory, request);
            var summary = response.Data;

            return summary;
        }

        [HttpPost]
        public async Task<string> Post(WeatherSummary data, [FromServices] IHttpClientFactory clientFactory)
        {
            var pathBase = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
            var baseUri = new Uri(pathBase);
            var requestUri = new Uri(baseUri, $"/api/qckmox/summary/{data.Id}");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            var response = await HttpHelper.SendHttpRequest<ApiResponse<WeatherSummary>>(clientFactory, request);
            var message = response.Message;

            return message;
        }
    }
}

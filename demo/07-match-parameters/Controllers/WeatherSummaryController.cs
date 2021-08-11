using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QckMox.Demo.MatchParameters.Http;
using QckMox.Demo.MatchParameters.Models;

namespace QckMox.Demo.MatchParameters.Controllers
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
            var requestUri = new Uri(baseUri, $"/api/qckmox/summary?id={id}");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await HttpHelper.SendHttpRequest<ApiResponse<WeatherSummary>>(clientFactory, request);
            var summary = response.Data;

            return summary;
        }

        [HttpPost]
        public async Task<string> Post(UpdateRequest data, [FromServices] IHttpClientFactory clientFactory)
        {
            var pathBase = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
            var baseUri = new Uri(pathBase);
            var query = string.IsNullOrWhiteSpace(data.QueryName) ? "id" : data.QueryName;
            var header = string.IsNullOrWhiteSpace(data.HeaderName) ? "user-id" : data.HeaderName;
            var requestUri = new Uri(baseUri, $"/api/qckmox/summary?{query}={data.QueryValue}");
            var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
            request.Headers.Add(header, $"{data.HeaderValue}");

            var response = await HttpHelper.SendHttpRequest<ApiResponse<WeatherSummary>>(clientFactory, request);
            var message = response.Message;

            return message;
        }
    }
}

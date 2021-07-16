using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QckMox.Demo.Simple.Http;
using QckMox.Demo.Simple.Models;

namespace QckMox.Demo.Simple.Controllers
{
    [ApiController]
    [Route("api/weatherforecast")]
    public class WeatherForecastController : ControllerBase
    {
        [HttpGet]
        public async Task<IEnumerable<WeatherForecast>> Get([FromServices] IHttpClientFactory clientFactory)
        {
            var pathBase = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}";
            var baseUri = new Uri(pathBase);
            var requestUri = new Uri(baseUri, "/api/qckmox/summary");
            var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

            var response = await HttpHelper.SendHttpRequest<ApiResponse<string[]>>(clientFactory, request);
            var summaries = response.Data.ToArray();

            var rng = new Random();
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = summaries[rng.Next(summaries.Length)]
            })
            .ToArray();

            return forecast;
        }
    }
}

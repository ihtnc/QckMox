using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using QckMox.Demo.Passthrough.Models;

namespace QckMox.Demo.Passthrough.Controllers
{
    [ApiController]
    [Route("api/weathersummary")]
    public class WeatherSummaryController : ControllerBase
    {
        [HttpGet]
        [Route("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var summary = await GetSummary(id);

            if (summary != null)
            {
                return Ok(summary);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<string> Post(WeatherSummary data)
        {
            return await Task.FromResult($"Summary (id: {data.Id}) updated!");
        }


        private Task<WeatherSummary> GetSummary(int id)
        {
            var summaries = new []
            {
                new WeatherSummary
                {
                    Id = 2,
                    Name = "Bracing"
                },
                new WeatherSummary
                {
                    Id = 4,
                    Name = "Cool"
                },
                new WeatherSummary
                {
                    Id = 6,
                    Name = "Warm"
                },
                new WeatherSummary
                {
                    Id = 8,
                    Name = "Hot"
                },
                new WeatherSummary
                {
                    Id = 10,
                    Name = "Scorching"
                }
            };

            var summary = summaries.SingleOrDefault(s => s.Id == id);
            return Task.FromResult(summary);
        }
    }
}

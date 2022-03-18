using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.ServiceFabric.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Parcels.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : Controller
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly IReliableStateManager stateManager;

        public WeatherForecastController(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var rng = new Random();
            var enumerable = Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Name = "Parcels",
                Date = DateTime.Now.AddDays(index),
                TemperatureC = rng.Next(-20, 55),
                Summary = Summaries[rng.Next(Summaries.Length)]
            });
            return this.Json(enumerable);
        }
    }
}

using DV8.Console.Services;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("analytics")]
    public class AnalyticsController : ControllerBase
    {
        private readonly AnalyticsService _service;
        public AnalyticsController(AnalyticsService service)
        {
            _service = service;
        }

        [HttpGet("latency")]
        public ActionResult<double> GetAverageLatency()
        {
            return _service.AverageLatency();
        }

        [HttpGet("status")]
        public ActionResult<Dictionary<string, int>> GetStatusDistribution()
        {
            return _service.StatusDistribution();
        }
    }
}

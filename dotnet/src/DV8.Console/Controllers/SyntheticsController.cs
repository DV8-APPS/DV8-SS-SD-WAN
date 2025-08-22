using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("synthetics")]
    public class SyntheticsController : ControllerBase
    {
        private readonly SyntheticsService _svc;
        public SyntheticsController(SyntheticsService svc) { _svc = svc; }

        [HttpPost("probes")]
        public object Register([FromBody] Dictionary<string, object> body)
            => _svc.Register(body["intentId"].ToString()!, new[] { "a" }, body["proto"].ToString()!, (int)body["interval"]);

        [HttpGet("results")]
        public IEnumerable<Dictionary<string, object>> Results([FromQuery] string intentId)
            => _svc.Results(intentId);
    }
}

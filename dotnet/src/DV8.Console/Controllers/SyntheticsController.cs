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
        {
            var missingKeys = new List<string>();
            if (!body.ContainsKey("intentId")) missingKeys.Add("intentId");
            if (!body.ContainsKey("proto")) missingKeys.Add("proto");
            if (!body.ContainsKey("interval")) missingKeys.Add("interval");
            if (missingKeys.Count > 0)
                return BadRequest($"Missing required key(s): {string.Join(", ", missingKeys)}");

            return _svc.Register(
                body["intentId"].ToString()!,
                new[] { "a" },
                body["proto"].ToString()!,
                (int)body["interval"]
            );
        }
        [HttpGet("results")]
        public IEnumerable<Dictionary<string, object>> Results([FromQuery] string intentId)
            => _svc.Results(intentId);
    }
}

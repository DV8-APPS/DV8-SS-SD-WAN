using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("guardrail")]
    public class GuardRailController : ControllerBase
    {
        private readonly GuardRailService _service;
        public GuardRailController(GuardRailService svc) { _service = svc; }

        [HttpPost("lint")]
        public object Lint([FromBody] Dictionary<string, string> body)
            => new { valid = _service.Lint(body["intentYaml"]) };

        [HttpPost("approve")]
        public object Approve([FromBody] Dictionary<string, string> body)
            => _service.Approve(body["changeId"]);
    }
}

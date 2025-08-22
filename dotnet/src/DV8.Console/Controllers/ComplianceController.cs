using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("compliance")]
    public class ComplianceController : ControllerBase
    {
        private readonly ComplianceService _service;
        public ComplianceController(ComplianceService service)
        {
            _service = service;
        }

        [HttpPost("scan")]
        public ActionResult<Dictionary<string,string>> Scan([FromBody] Dictionary<string,string> req)
        {
            return _service.StartScan(req["profile"]);
        }

        [HttpGet("results")]
        public ActionResult<Dictionary<string,string>> Results([FromQuery] string profile, [FromQuery] string device)
        {
            return _service.Results(profile, device);
        }
    }
}

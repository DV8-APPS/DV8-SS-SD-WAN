using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("vulnwatch")]
    public class VulnWatchController : ControllerBase
    {
        private readonly VulnWatchService _service;
        public VulnWatchController(VulnWatchService service)
        {
            _service = service;
        }

        [HttpPost("sync")]
        public ActionResult<Dictionary<string,string>> Sync()
        {
            return _service.Sync();
        }

        [HttpGet("findings")]
        public ActionResult<Dictionary<string,object>> Findings([FromQuery] string device)
        {
            return _service.Findings(device);
        }
    }
}

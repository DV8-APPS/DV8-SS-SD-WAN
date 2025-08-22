using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("sdwan")]
    public class PathTracerController : ControllerBase
    {
        private readonly PathTracerService _service;
        public PathTracerController(PathTracerService service)
        {
            _service = service;
        }

        [HttpGet("pathtracer")]
        public ActionResult<Dictionary<string,object>> Trace([FromQuery] string src, [FromQuery] string dst)
        {
            return _service.Trace(src, dst);
        }
    }
}

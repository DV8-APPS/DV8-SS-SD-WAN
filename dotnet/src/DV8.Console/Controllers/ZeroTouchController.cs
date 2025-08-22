using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("zero-touch")]
    public class ZeroTouchController : ControllerBase
    {
        private readonly ZeroTouchService _service;
        public ZeroTouchController(ZeroTouchService service)
        {
            _service = service;
        }

        [HttpPost("enroll")]
        public IActionResult Enroll([FromBody] ZeroTouchRequest req)
        {
            var result = _service.Enroll(req.Name, req.Template);
            return Ok(result);
        }

        [HttpPost("device/{name}/applied")]
        public IActionResult MarkApplied(string name)
        {
            var result = _service.MarkApplied(name);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("device/{name}")]
        public IActionResult Get(string name)
        {
            var result = _service.Get(name);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpGet("devices")]
        public IActionResult List()
        {
            return Ok(_service.List());
        }
    }

    public record ZeroTouchRequest(string Name, string Template);
}

using Microsoft.AspNetCore.Mvc;
using DV8.Console.Models;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("sandbox")]
    public class SandboxController : ControllerBase
    {
        private readonly SandboxManager _manager;
        public SandboxController(SandboxManager manager)
        {
            _manager = manager;
        }

        [HttpPost("device")]
        public ActionResult<SandboxDevice> Register(SandboxDevice device)
        {
            return _manager.Register(device);
        }

        [HttpGet("devices")]
        public IEnumerable<SandboxDevice> List() => _manager.List();

        [HttpGet("device/{name}")]
        public ActionResult<SandboxDevice> Get(string name)
        {
            var device = _manager.Get(name);
            if (device == null)
            {
                return NotFound();
            }
            return device;
        }

        [HttpPut("device/{name}")]
        public ActionResult<SandboxDevice> Update(string name, StatusUpdate update)
        {
            var device = _manager.Update(name, update);
            if (device == null)
            {
                return NotFound();
            }
            return device;
        }

        [HttpGet("summary")]
        public object Summary() => _manager.Summary();
    }
}

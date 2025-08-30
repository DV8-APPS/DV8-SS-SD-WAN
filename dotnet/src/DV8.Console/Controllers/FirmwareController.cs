using DV8.Console.Models;
using DV8.Console.Services;
using Microsoft.AspNetCore.Mvc;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("firmware")]
    public class FirmwareController : ControllerBase
    {
        private readonly FirmwareManager _manager;
        public FirmwareController(FirmwareManager manager)
        {
            _manager = manager;
        }

        [HttpPost("install")]
        public IActionResult Install(FirmwareInstallRequest request)
        {
            _manager.Install(request.DeviceId, request.Version);
            return Ok(new { deviceId = request.DeviceId, version = request.Version });
        }

        [HttpGet("version/{deviceId}")]
        public IActionResult GetVersion(string deviceId)
        {
            var version = _manager.GetVersion(deviceId);
            if (version == null)
            {
                return NotFound();
            }
            return Ok(new { deviceId, version });
        }
    }
}

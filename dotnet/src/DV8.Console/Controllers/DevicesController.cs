using DV8.Console.Data;
using DV8.Console.Services;
using Microsoft.AspNetCore.Mvc;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DevicesController : ControllerBase
    {
        private readonly DeviceService _deviceService;

        public DevicesController(DeviceService deviceService)
        {
            _deviceService = deviceService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Device>>> GetDevices([FromQuery] string? tenant = null, [FromQuery] string? site = null)
        {
            try
            {
                IEnumerable<Device> devices;
                
                if (!string.IsNullOrEmpty(tenant))
                {
                    devices = await _deviceService.GetDevicesByTenantAsync(tenant);
                }
                else if (!string.IsNullOrEmpty(site))
                {
                    devices = await _deviceService.GetDevicesBySiteAsync(site);
                }
                else
                {
                    devices = await _deviceService.GetAllDevicesAsync();
                }

                return Ok(devices);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve devices", details = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Device>> GetDevice(Guid id)
        {
            try
            {
                var device = await _deviceService.GetDeviceByIdAsync(id);
                if (device == null)
                {
                    return NotFound(new { error = "Device not found" });
                }
                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve device", details = ex.Message });
            }
        }

        [HttpGet("fingerprint/{fingerprint}")]
        public async Task<ActionResult<Device>> GetDeviceByFingerprint(string fingerprint)
        {
            try
            {
                var device = await _deviceService.GetDeviceByFingerprintAsync(fingerprint);
                if (device == null)
                {
                    return NotFound(new { error = "Device not found" });
                }
                return Ok(device);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve device", details = ex.Message });
            }
        }

        [HttpPost]
        public async Task<ActionResult<Device>> CreateDevice([FromBody] CreateDeviceRequest request)
        {
            try
            {
                var device = new Device
                {
                    Tenant = request.Tenant,
                    Kind = request.Kind,
                    Vendor = request.Vendor,
                    Model = request.Model,
                    SwVersion = request.SwVersion,
                    Fingerprint = request.Fingerprint,
                    Site = request.Site,
                    Posture = request.Posture ?? "ECC"
                };

                var createdDevice = await _deviceService.CreateDeviceAsync(device);
                return CreatedAtAction(nameof(GetDevice), new { id = createdDevice.Id }, createdDevice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to create device", details = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Device>> UpdateDevice(Guid id, [FromBody] UpdateDeviceRequest request)
        {
            try
            {
                var device = new Device
                {
                    Tenant = request.Tenant,
                    Kind = request.Kind,
                    Vendor = request.Vendor,
                    Model = request.Model,
                    SwVersion = request.SwVersion,
                    Site = request.Site,
                    Posture = request.Posture,
                    TrustState = request.TrustState
                };

                var updatedDevice = await _deviceService.UpdateDeviceAsync(id, device);
                if (updatedDevice == null)
                {
                    return NotFound(new { error = "Device not found" });
                }

                return Ok(updatedDevice);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to update device", details = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDevice(Guid id)
        {
            try
            {
                var deleted = await _deviceService.DeleteDeviceAsync(id);
                if (!deleted)
                {
                    return NotFound(new { error = "Device not found" });
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to delete device", details = ex.Message });
            }
        }
    }

    public class CreateDeviceRequest
    {
        public string Tenant { get; set; } = string.Empty;
        public string? Kind { get; set; }
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? SwVersion { get; set; }
        public string? Fingerprint { get; set; }
        public string? Site { get; set; }
        public string? Posture { get; set; }
    }

    public class UpdateDeviceRequest
    {
        public string Tenant { get; set; } = string.Empty;
        public string? Kind { get; set; }
        public string? Vendor { get; set; }
        public string? Model { get; set; }
        public string? SwVersion { get; set; }
        public string? Site { get; set; }
        public string? Posture { get; set; }
        public string? TrustState { get; set; }
    }
}
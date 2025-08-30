using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("heal")]
    public class AutoHealerController : ControllerBase
    {
        private readonly AutoHealerService _service;
        public AutoHealerController(AutoHealerService svc) { _service = svc; }

        [HttpPost("playbooks")]
        {
            if (body == null || !body.TryGetValue("name", out var nameObj) || nameObj == null)
                return BadRequest(new { error = "'name' field is required in the request body." });
            return _service.CreatePlaybook(nameObj.ToString()!);
        }

        [HttpGet("incidents")]
        public IEnumerable<Dictionary<string, object>> ListIncidents()
            => _service.ListIncidents();

        [HttpPost("incidents/{id}/execute")]
        public ActionResult<Dictionary<string, object>> Execute(int id)
        {
            var result = _service.ExecuteIncident(id);
            if (result == null) return NotFound();
            return result;
        }
    }
}

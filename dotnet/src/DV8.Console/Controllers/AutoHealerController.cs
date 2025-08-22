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
        public ActionResult<Dictionary<string, object>> CreatePlaybook([FromBody] Dictionary<string, object> body)
            => _service.CreatePlaybook(body["name"].ToString()!);

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

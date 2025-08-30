using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("discovery")]
    public class DiscoveryController : ControllerBase
    {
        private readonly DiscoveryService _service;
        public DiscoveryController(DiscoveryService service)
        {
            _service = service;
        }

        public record StartRequest(string Tenant, IEnumerable<string> Scopes, IEnumerable<string> Cidrs, bool PassiveOnly);

        [HttpPost("jobs")]
        public ActionResult<DiscoveryJob> StartJob(StartRequest req)
        {
            var job = _service.Start(req.Tenant, req.Scopes, req.Cidrs, req.PassiveOnly);
            return Accepted(job);
        }

        [HttpGet("jobs/{id}")]
        public ActionResult<DiscoveryJob> GetJob(string id)
        {
            var job = _service.Get(id);
            if (job == null)
            {
                return NotFound();
            }
            return job;
        }

        [HttpGet("candidates")]
        public IEnumerable<Candidate> Candidates([FromQuery] string tenant)
        {
            return _service.Candidates(tenant);
        }
    }
}

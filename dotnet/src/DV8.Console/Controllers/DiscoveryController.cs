using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using DV8.Console.Data;
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

        public record StartRequest(string Tenant, IEnumerable<string> Scopes, IEnumerable<string> Cidrs, bool PassiveOnly, string? ByUser = null, string? DbgSession = null);

        [HttpPost("jobs")]
        public async Task<ActionResult<DiscoveryJob>> StartJob(StartRequest req)
        {
            try
            {
                var job = await _service.StartAsync(req.Tenant, req.Scopes, req.Cidrs, req.PassiveOnly, req.ByUser, req.DbgSession);
                return Accepted(job);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to start discovery job", details = ex.Message });
            }
        }

        [HttpGet("jobs/{id}")]
        public async Task<ActionResult<DiscoveryJob>> GetJob(Guid id)
        {
            try
            {
                var job = await _service.GetAsync(id);
                if (job == null)
                {
                    return NotFound(new { error = "Discovery job not found" });
                }
                return job;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve discovery job", details = ex.Message });
            }
        }

        [HttpGet("jobs")]
        public async Task<ActionResult<IEnumerable<DiscoveryJob>>> GetJobs([FromQuery] string? tenant = null)
        {
            try
            {
                IEnumerable<DiscoveryJob> jobs;
                if (!string.IsNullOrEmpty(tenant))
                {
                    jobs = await _service.GetByTenantAsync(tenant);
                }
                else
                {
                    jobs = await _service.GetAllJobsAsync();
                }
                return Ok(jobs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve discovery jobs", details = ex.Message });
            }
        }

        [HttpPost("jobs/{id}/results")]
        public async Task<ActionResult<DiscoveryResult>> AddResult(Guid id, [FromBody] AddResultRequest request)
        {
            try
            {
                var result = await _service.AddResultAsync(id, request.Fingerprint, request.Addrs, request.Signals, request.Confidence, request.Raw);
                return CreatedAtAction(nameof(GetJob), new { id }, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to add discovery result", details = ex.Message });
            }
        }

        [HttpGet("candidates")]
        public async Task<ActionResult<IEnumerable<CandidateInfo>>> Candidates([FromQuery] string tenant)
        {
            try
            {
                var candidates = await _service.GetCandidatesAsync(tenant);
                return Ok(candidates);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Failed to retrieve candidates", details = ex.Message });
            }
        }
    }

    public record AddResultRequest(string Fingerprint, IEnumerable<string> Addrs, IEnumerable<string> Signals, float Confidence, object? Raw = null);
}

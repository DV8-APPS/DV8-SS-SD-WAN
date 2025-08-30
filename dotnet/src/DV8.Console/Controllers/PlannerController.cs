using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("planner")]
    public class PlannerController : ControllerBase
    {
        private readonly PlannerService _svc;
        public PlannerController(PlannerService svc) { _svc = svc; }

        [HttpPost("recommend")]
        {
            var missingKeys = new List<string>();
            if (!body.ContainsKey("site")) missingKeys.Add("site");
            if (!body.ContainsKey("intentId")) missingKeys.Add("intentId");
            if (!body.ContainsKey("horizonDays")) missingKeys.Add("horizonDays");
            if (missingKeys.Count > 0)
            {
                return BadRequest(new { error = $"Missing required key(s): {string.Join(", ", missingKeys)}" });
            }
            return _svc.Recommend(body["site"].ToString()!, body["intentId"].ToString()!, (int)body["horizonDays"]);
        }
    }
}

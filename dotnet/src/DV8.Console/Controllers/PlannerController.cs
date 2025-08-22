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
        public object Recommend([FromBody] Dictionary<string, object> body)
            => _svc.Recommend(body["site"].ToString()!, body["intentId"].ToString()!, (int)body["horizonDays"]);
    }
}

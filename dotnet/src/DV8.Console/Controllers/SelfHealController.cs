using Microsoft.AspNetCore.Mvc;
using DV8.Console.Services;
using System.Collections.Generic;

namespace DV8.Console.Controllers
{
    [ApiController]
    [Route("selfheal")]
    public class SelfHealController : ControllerBase
    {
        private readonly SelfHealService _service;
        public SelfHealController(SelfHealService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<IEnumerable<string>> Heal()
        {
            return Ok(_service.Heal());
        }
    }
}

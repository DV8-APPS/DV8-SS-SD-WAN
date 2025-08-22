using System.Collections.Generic;
using DV8.Console.Models;

namespace DV8.Console.Services
{
    public class SelfHealService
    {
        private readonly SandboxManager _sandbox;
        private readonly QuantumShieldService _qs;
        public SelfHealService(SandboxManager sandbox, QuantumShieldService qs)
        {
            _sandbox = sandbox;
            _qs = qs;
        }

        public IEnumerable<string> Heal()
        {
            var healed = new List<string>();
            foreach (var d in _sandbox.List())
            {
                if (d.Status == "down" && _qs.RiskScore(d.Status) < 80)
                {
                    d.Status = "up";
                    healed.Add(d.Name);
                }
            }
            return healed;
        }
    }
}

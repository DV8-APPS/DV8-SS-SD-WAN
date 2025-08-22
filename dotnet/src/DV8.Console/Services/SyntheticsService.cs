using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class SyntheticsService
    {
        private readonly List<Dictionary<string, object>> _probes = new();

        public Dictionary<string, object> Register(string intentId, IEnumerable<string> endpoints, string proto, int interval)
        {
            var probe = new Dictionary<string, object>{{"id", _probes.Count + 1}, {"intentId", intentId}, {"proto", proto}, {"interval", interval}};
            _probes.Add(probe);
            return probe;
        }

        public IEnumerable<Dictionary<string, object>> Results(string intentId)
        {
            return new List<Dictionary<string, object>>
            {
                new() { {"intentId", intentId}, {"endpoint", "synthetic"}, {"latencyMs", 10} }
            };
        }
    }
}

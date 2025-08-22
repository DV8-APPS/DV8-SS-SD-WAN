using System;
using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class DiscoveryService
    {
        private readonly Dictionary<string, DiscoveryJob> _jobs = new();
        private readonly List<Candidate> _candidates = new();

        public DiscoveryJob Start(string tenant, IEnumerable<string> scopes, IEnumerable<string> cidrs, bool passiveOnly)
        {
            var job = new DiscoveryJob
            {
                Id = Guid.NewGuid().ToString(),
                Status = "running",
                StartedAt = DateTime.UtcNow,
                Tenant = tenant,
                Scopes = new List<string>(scopes),
                Cidrs = new List<string>(cidrs),
                PassiveOnly = passiveOnly
            };
            _jobs[job.Id] = job;
            return job;
        }

        public DiscoveryJob? Get(string id)
        {
            return _jobs.TryGetValue(id, out var job) ? job : null;
        }

        public IEnumerable<Candidate> Candidates(string tenant)
        {
            return _candidates;
        }
    }

    public class DiscoveryJob
    {
        public string Id { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public string Tenant { get; set; } = string.Empty;
        public List<string> Scopes { get; set; } = new();
        public List<string> Cidrs { get; set; } = new();
        public bool PassiveOnly { get; set; }
    }

    public class Candidate
    {
        public string Fp { get; set; } = string.Empty;
        public List<string> Addrs { get; set; } = new();
        public List<string> Signals { get; set; } = new();
        public float Confidence { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text.Json;
using DV8.Console.Data;
using Microsoft.EntityFrameworkCore;

namespace DV8.Console.Services
{
    public class DiscoveryService
    {
        private readonly DV8DbContext _context;

        public DiscoveryService(DV8DbContext context)
        {
            _context = context;
        }

        public async Task<DiscoveryJob> StartAsync(string tenant, IEnumerable<string> scopes, IEnumerable<string> cidrs, bool passiveOnly, string? byUser = null, string? dbgSession = null)
        {
            var job = new DiscoveryJob
            {
                Id = Guid.NewGuid(),
                Tenant = tenant,
                Cidrs = JsonSerializer.Serialize(cidrs),
                PassiveOnly = passiveOnly,
                StartedAt = DateTime.UtcNow,
                ByUser = byUser,
                DbgSession = dbgSession
            };

            _context.DiscoveryJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<DiscoveryJob?> GetAsync(Guid id)
        {
            return await _context.DiscoveryJobs
                .Include(j => j.Results)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<IEnumerable<DiscoveryJob>> GetByTenantAsync(string tenant)
        {
            return await _context.DiscoveryJobs
                .Where(j => j.Tenant == tenant)
                .Include(j => j.Results)
                .OrderByDescending(j => j.StartedAt)
                .ToListAsync();
        }

        public async Task<DiscoveryResult> AddResultAsync(Guid jobId, string fingerprint, IEnumerable<string> addrs, IEnumerable<string> signals, float confidence, object? raw = null)
        {
            var result = new DiscoveryResult
            {
                JobId = jobId,
                Fingerprint = fingerprint,
                Addrs = JsonSerializer.Serialize(addrs),
                Signals = JsonSerializer.Serialize(signals),
                Confidence = confidence,
                Raw = raw != null ? JsonSerializer.Serialize(raw) : null
            };

            _context.DiscoveryResults.Add(result);
            await _context.SaveChangesAsync();
            return result;
        }

        public async Task<IEnumerable<CandidateInfo>> GetCandidatesAsync(string tenant)
        {
            var results = await _context.DiscoveryResults
                .Include(r => r.Job)
                .Where(r => r.Job.Tenant == tenant)
                .ToListAsync();

            var candidates = results
                .GroupBy(r => r.Fingerprint)
                .Select(g => new CandidateInfo
                {
                    Fingerprint = g.Key,
                    Addresses = g.SelectMany(r => 
                        {
                            var addrs = r.Addrs ?? "[]";
                            return JsonSerializer.Deserialize<string[]>(addrs) ?? Array.Empty<string>();
                        }).Distinct().ToList(),
                    Signals = g.SelectMany(r => 
                        {
                            var signals = r.Signals ?? "[]";
                            return JsonSerializer.Deserialize<string[]>(signals) ?? Array.Empty<string>();
                        }).Distinct().ToList(),
                    Confidence = g.Max(r => r.Confidence ?? 0),
                    LastSeen = g.Max(r => r.Job.StartedAt)
                })
                .ToList();

            return candidates;
        }

        public async Task<IEnumerable<DiscoveryJob>> GetAllJobsAsync()
        {
            return await _context.DiscoveryJobs
                .Include(j => j.Results)
                .OrderByDescending(j => j.StartedAt)
                .ToListAsync();
        }
    }

    public class CandidateInfo
    {
        public string Fingerprint { get; set; } = string.Empty;
        public List<string> Addresses { get; set; } = new();
        public List<string> Signals { get; set; } = new();
        public float Confidence { get; set; }
        public DateTime LastSeen { get; set; }
    }

    // Keep legacy classes for backward compatibility if needed
    public class Candidate
    {
        public string Fp { get; set; } = string.Empty;
        public List<string> Addrs { get; set; } = new();
        public List<string> Signals { get; set; } = new();
        public float Confidence { get; set; }
    }
}

using System.Collections.Generic;
using System.Linq;

namespace DV8.Console.Services
{
    public class AnalyticsService
    {
        private readonly List<int> _latencies = new() { 10, 20, 30 };
        private readonly List<string> _statuses = new() { "online", "offline", "offline", "maintenance" };

        public double AverageLatency()
        {
            return _latencies.Any() ? _latencies.Average() : 0;
        }

        public Dictionary<string, int> StatusDistribution()
        {
            return _statuses
                .GroupBy(s => s)
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }
}

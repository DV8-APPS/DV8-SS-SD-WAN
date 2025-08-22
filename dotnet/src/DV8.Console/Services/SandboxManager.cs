using System.Collections.Generic;
using System.Linq;
using DV8.Console.Models;

namespace DV8.Console.Services
{
    public class SandboxManager
    {
        private readonly Dictionary<string, SandboxDevice> _devices = new();

        public SandboxDevice Register(SandboxDevice device)
        {
            _devices[device.Name] = device;
            return device;
        }

        public IEnumerable<SandboxDevice> List() => _devices.Values;

        public SandboxDevice? Get(string name)
        {
            _devices.TryGetValue(name, out var device);
            return device;
        }

        public SandboxDevice? Update(string name, StatusUpdate update)
        {
            if (!_devices.TryGetValue(name, out var device))
            {
                return null;
            }
            if (update.Status != null)
            {
                device.Status = update.Status;
            }
            if (update.Warnings != null)
            {
                foreach (var kv in update.Warnings)
                {
                    device.Warnings[kv.Key] = kv.Value;
                }
            }
            return device;
        }

        public object Summary()
        {
            var totalPorts = _devices.Values.Sum(d => d.Ports);
            var statusCounts = _devices.Values
                .GroupBy(d => d.Status)
                .ToDictionary(g => g.Key, g => g.Count());
            var warningCounts = new Dictionary<string, int>();
            foreach (var d in _devices.Values)
            {
                foreach (var w in d.Warnings.Where(kv => kv.Value))
                {
                    warningCounts[w.Key] = warningCounts.GetValueOrDefault(w.Key) + 1;
                }
            }
            return new
            {
                totalDevices = _devices.Count,
                totalPorts,
                statusCounts,
                warningCounts
            };
        }
    }
}

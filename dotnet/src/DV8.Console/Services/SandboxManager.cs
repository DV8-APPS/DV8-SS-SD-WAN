using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using DV8.Console.Models;

namespace DV8.Console.Services
{
    public class SandboxManager
    {
        private readonly ConcurrentDictionary<string, SandboxDevice> _devices = new();

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
            if (!_devices.TryGetValue(name, out var device)) return null;
            if (update.Status is not null) device.Status = update.Status;
            if (update.Warnings is not null)
            {
                foreach (var kv in update.Warnings)
                {
                    device.Warnings[kv.Key] = kv.Value;
                }
            }
            if (update.Latitude is not null) device.Latitude = update.Latitude;
            if (update.Longitude is not null) device.Longitude = update.Longitude;
            return device;
        }

        public object Summary()
        {
            var devices = _devices.Values;
            var statusCounts = devices.GroupBy(d => d.Status)
                                      .ToDictionary(g => g.Key, g => g.Count());
            var warningCounts = devices
                .SelectMany(d => d.Warnings.Where(w => w.Value).Select(w => w.Key))
                .GroupBy(name => name)
                .ToDictionary(g => g.Key, g => g.Count());
            return new
            {
                totalDevices = devices.Count(),
                totalPorts = devices.Sum(d => d.Ports),
                statusCounts,
                warningCounts
            };
        }
    }
}

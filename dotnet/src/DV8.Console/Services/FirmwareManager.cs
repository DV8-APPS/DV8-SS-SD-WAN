using System.Collections.Concurrent;

namespace DV8.Console.Services
{
    public class FirmwareManager
    {
        private readonly ConcurrentDictionary<string, string> _versions = new();

        public void Install(string deviceId, string version)
        {
            _versions[deviceId] = version;
        }

        public string? GetVersion(string deviceId)
        {
            return _versions.TryGetValue(deviceId, out var v) ? v : null;
        }
    }
}

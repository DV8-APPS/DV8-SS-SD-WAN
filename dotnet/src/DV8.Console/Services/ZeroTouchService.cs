using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class ZeroTouchService
    {
        private readonly Dictionary<string, (string Template, bool Applied)> _devices = new();

        public object Enroll(string name, string template)
        {
            if (_devices.ContainsKey(name))
            {
                var existing = _devices[name];
                _devices[name] = (template, existing.Applied);
            }
            else
            {
                _devices[name] = (template, false);
            }
            return new { name, template, applied = _devices[name].Applied };
        }

        public object? Get(string name)
        {
            if (_devices.TryGetValue(name, out var info))
                return new { name, template = info.Template, applied = info.Applied };
            return null;
        }

        public IEnumerable<object> List()
        {
            foreach (var kv in _devices)
                yield return new { name = kv.Key, template = kv.Value.Template, applied = kv.Value.Applied };
        }

        public object? MarkApplied(string name)
        {
            if (_devices.TryGetValue(name, out var info))
            {
                _devices[name] = (info.Template, true);
                return new { name, template = info.Template, applied = true };
            }
            return null;
        }
    }
}

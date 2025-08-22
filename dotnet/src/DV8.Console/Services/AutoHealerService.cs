using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class AutoHealerService
    {
        private readonly Dictionary<int, Dictionary<string, object>> _playbooks = new();
        private readonly Dictionary<int, Dictionary<string, object>> _incidents = new();

        public Dictionary<string, object> CreatePlaybook(string name)
        {
            var id = _playbooks.Count + 1;
            var pb = new Dictionary<string, object>{{"id", id}, {"name", name}};
            _playbooks[id] = pb;
            _incidents[id] = new Dictionary<string, object>{{"id", id}, {"playbookId", id}, {"state", "open"}};
            return pb;
        }

        public IEnumerable<Dictionary<string, object>> ListIncidents()
            => _incidents.Values;

        public Dictionary<string, object>? ExecuteIncident(int id)
        {
            if (_incidents.TryGetValue(id, out var inc) && (string)inc["state"] == "open")
            {
                inc["state"] = "closed";
                return inc;
            }
            return null;
        }
    }
}

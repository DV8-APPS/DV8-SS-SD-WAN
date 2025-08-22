using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class VulnWatchService
    {
        public Dictionary<string,string> Sync()
        {
            return new Dictionary<string,string>{{"status","synced"}};
        }

        public Dictionary<string,object> Findings(string device)
        {
            return new Dictionary<string,object>{{"device", device}, {"vulnerabilities", new List<string>()}};
        }
    }
}

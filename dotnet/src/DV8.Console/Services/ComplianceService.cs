using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class ComplianceService
    {
        public Dictionary<string,string> StartScan(string profile)
        {
            return new Dictionary<string,string>{{"job", profile + "-scan"}};
        }

        public Dictionary<string,string> Results(string profile, string device)
        {
            return new Dictionary<string,string>{{"device", device}, {"profile", profile}, {"status", "pass"}};
        }
    }
}

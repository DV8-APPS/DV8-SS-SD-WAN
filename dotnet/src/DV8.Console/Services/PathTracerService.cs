using System.Collections.Generic;

namespace DV8.Console.Services
{
    public class PathTracerService
    {
        public Dictionary<string,object> Trace(string src, string dst)
        {
            var hops = new List<Dictionary<string,int>>
            {
                new Dictionary<string,int>{{"hop",1},{"latency_ms",5}},
                new Dictionary<string,int>{{"hop",2},{"latency_ms",10}}
            };
            return new Dictionary<string,object>{{"src",src},{"dst",dst},{"hops",hops}};
        }
    }
}

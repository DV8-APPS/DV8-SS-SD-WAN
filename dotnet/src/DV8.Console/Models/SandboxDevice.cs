using System.Collections.Generic;

namespace DV8.Console.Models
{
    public class SandboxDevice
    {
        public string Name { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public int Ports { get; set; }
        public string Status { get; set; } = "up";
        public Dictionary<string, bool> Warnings { get; set; } = new();
    }

    public class StatusUpdate
    {
        public string? Status { get; set; }
        public Dictionary<string, bool>? Warnings { get; set; }
    }
}

namespace DV8.Console.Services
{
    public class GuardRailService
    {
        public bool Lint(string intentYaml) => intentYaml.Contains("policy");
        public object Approve(string changeId) => new { changeId, status = "approved" };
    }
}

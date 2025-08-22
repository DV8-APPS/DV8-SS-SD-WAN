using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using Xunit;

namespace DV8.Console.Tests
{
    public class GuardRailControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public GuardRailControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task LintAndApprove()
        {
            var lint = await _client.PostAsJsonAsync("/guardrail/lint", new { intentYaml = "policy: allow" });
            lint.EnsureSuccessStatusCode();
            var approve = await _client.PostAsJsonAsync("/guardrail/approve", new { changeId = "1" });
            approve.EnsureSuccessStatusCode();
        }
    }
}

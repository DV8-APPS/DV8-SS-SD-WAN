using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Threading.Tasks;
using Xunit;

namespace DV8.Console.Tests
{
    public class AutoHealerControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;
        public AutoHealerControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task PlaybookLifecycle()
        {
            var pb = await _client.PostAsJsonAsync("/heal/playbooks", new { name = "pb" });
            pb.EnsureSuccessStatusCode();
            var incs = await _client.GetFromJsonAsync<System.Collections.Generic.List<System.Collections.Generic.Dictionary<string, object>>>("/heal/incidents");
            Assert.NotNull(incs);
            var exec = await _client.PostAsync("/heal/incidents/1/execute", null);
            Assert.True(exec.IsSuccessStatusCode);
        }
    }
}

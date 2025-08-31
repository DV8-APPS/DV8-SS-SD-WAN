using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DV8.Console.Tests
{
    public class FirmwareControllerTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public FirmwareControllerTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task InstallAndRetrieveVersion()
        {
            var installResponse = await _client.PostAsJsonAsync("/firmware/install", new { DeviceId = "r1", Version = "1.0" });
            installResponse.EnsureSuccessStatusCode();

            var versionResponse = await _client.GetAsync("/firmware/version/r1");
            versionResponse.EnsureSuccessStatusCode();
            var payload = await versionResponse.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            Assert.NotNull(payload);
            Assert.Equal("1.0", payload["version"]);
        }

        [Fact]
        public async Task AnalyticsEndpointsReturnData()
        {
            var latency = await _client.GetFromJsonAsync<double>("/analytics/latency");
            Assert.True(latency > 0);

            var status = await _client.GetFromJsonAsync<Dictionary<string, int>>("/analytics/status");
            Assert.True(status?.ContainsKey("offline") ?? false);
        }
    }
}

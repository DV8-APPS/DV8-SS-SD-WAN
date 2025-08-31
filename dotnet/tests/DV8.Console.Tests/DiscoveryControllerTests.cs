using System.Collections.Generic;
using System.Net.Http.Json;
using System.Threading.Tasks;
using DV8.Console.Services;
using DV8.Console.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace DV8.Console.Tests;

public class DiscoveryControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public DiscoveryControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task StartJobAndGetStatus()
    {
        var request = new { Tenant = "t1", Scopes = new[] { "site:A" }, Cidrs = new[] { "10.0.0.0/24" }, PassiveOnly = false };
        var resp = await _client.PostAsJsonAsync("/discovery/jobs", request);
        Assert.Equal(System.Net.HttpStatusCode.Accepted, resp.StatusCode);
        var job = await resp.Content.ReadFromJsonAsync<DiscoveryJob>();
        Assert.NotNull(job);
        var status = await _client.GetAsync($"/discovery/jobs/{job!.Id}");
        status.EnsureSuccessStatusCode();
        var list = await _client.GetFromJsonAsync<IEnumerable<CandidateInfo>>("/discovery/candidates?tenant=t1");
        Assert.NotNull(list);
    }
}

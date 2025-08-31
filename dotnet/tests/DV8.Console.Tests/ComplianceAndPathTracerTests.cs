using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DV8.Console.Tests;

public class ComplianceAndPathTracerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    public ComplianceAndPathTracerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task ComplianceScanAndResults()
    {
        var scan = await _client.PostAsJsonAsync("/compliance/scan", new Dictionary<string,string>{{"profile","DISA"}});
        scan.EnsureSuccessStatusCode();
        var results = await _client.GetAsync("/compliance/results?profile=DISA&device=r1");
        results.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task PathTracerEndpoint()
    {
        var trace = await _client.GetAsync("/sdwan/pathtracer?src=a&dst=b");
        trace.EnsureSuccessStatusCode();
    }
}

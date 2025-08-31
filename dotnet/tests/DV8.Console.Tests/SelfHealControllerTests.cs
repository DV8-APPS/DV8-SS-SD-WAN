using System.Net.Http.Json;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Testing;
using DV8.Console.Models;
using Xunit;

namespace DV8.Console.Tests;

public class SelfHealControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    public SelfHealControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SelfHealRecoversDevice()
    {
        var device = new SandboxDevice { Name = "heal-router", DeviceType = "router", Ports = 4, Status = "down" };
        var resp = await _client.PostAsJsonAsync("/sandbox/device", device);
        resp.EnsureSuccessStatusCode();
        var healResp = await _client.PostAsync("/selfheal", null);
        healResp.EnsureSuccessStatusCode();
        var healed = await healResp.Content.ReadFromJsonAsync<List<string>>();
        Assert.Contains("heal-router", healed ?? new List<string>());
    }
}

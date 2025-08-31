using System.Net.Http.Json;
using System.Collections.Generic;
using DV8.Console.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DV8.Console.Tests;

public class SandboxControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SandboxControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeviceLifecycle()
    {
        var device = new SandboxDevice { Name = "sim-router", DeviceType = "router", Ports = 8 };
        var resp = await _client.PostAsJsonAsync("/sandbox/device", device);
        resp.EnsureSuccessStatusCode();

        var update = new StatusUpdate { Status = "down", Warnings = new Dictionary<string, bool> { ["temperature"] = true } };
        var resp2 = await _client.PutAsJsonAsync("/sandbox/device/sim-router", update);
        resp2.EnsureSuccessStatusCode();
        var updated = await resp2.Content.ReadFromJsonAsync<SandboxDevice>();
        Assert.Equal("down", updated!.Status);
        Assert.True(updated.Warnings["temperature"]);

        var summary = await _client.GetFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>("/sandbox/summary");
        Assert.NotNull(summary);
        Assert.True(summary!["totalDevices"].GetInt32() >= 1);
    }
}

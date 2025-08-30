using System.Net.Http.Json;
using System.Collections.Generic;
using DV8.Console.Models;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DV8.Console.Tests;

public class SandboxControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public SandboxControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task DeviceLifecycle()
    {
        var device = new SandboxDevice { Name = "sim-router", DeviceType = "router", Ports = 8, Latitude = -33.9, Longitude = 18.4 };
        var resp = await _client.PostAsJsonAsync("/sandbox/device", device);
        resp.EnsureSuccessStatusCode();

        var update = new StatusUpdate { Status = "down", Warnings = new Dictionary<string, bool> { ["temperature"] = true }, Latitude = -34.0, Longitude = 18.5 };
        var resp2 = await _client.PutAsJsonAsync("/sandbox/device/sim-router", update);
        resp2.EnsureSuccessStatusCode();
        var updated = await resp2.Content.ReadFromJsonAsync<SandboxDevice>();
        Assert.Equal("down", updated!.Status);
        Assert.True(updated.Warnings["temperature"]);
        Assert.Equal(-34.0, updated.Latitude);
        Assert.Equal(18.5, updated.Longitude);

        var summary = await _client.GetFromJsonAsync<Dictionary<string, System.Text.Json.JsonElement>>("/sandbox/summary");
        Assert.NotNull(summary);
        Assert.True(summary!["totalDevices"].GetInt32() >= 1);
    }
}

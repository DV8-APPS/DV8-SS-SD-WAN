using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;

namespace DV8.Console.Tests;

public class ZeroTouchControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ZeroTouchControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task EnrollAndApply()
    {
        var resp = await _client.PostAsJsonAsync("/zero-touch/enroll", new { Name = "ztp-router", Template = "basic-router" });
        resp.EnsureSuccessStatusCode();
        var body = await resp.Content.ReadFromJsonAsync<ZeroTouchResponse>();
        Assert.NotNull(body);
        Assert.Equal("basic-router", body!.Template);
        Assert.False(body.Applied);

        var applied = await _client.PostAsync("/zero-touch/device/ztp-router/applied", null);
        applied.EnsureSuccessStatusCode();
        var appliedBody = await applied.Content.ReadFromJsonAsync<ZeroTouchResponse>();
        Assert.NotNull(appliedBody);
        Assert.True(appliedBody!.Applied);

        var list = await _client.GetFromJsonAsync<List<ZeroTouchResponse>>("/zero-touch/devices");
        Assert.True(list!.Count >= 1);
    }
}

public record ZeroTouchResponse(string Name, string Template, bool Applied);

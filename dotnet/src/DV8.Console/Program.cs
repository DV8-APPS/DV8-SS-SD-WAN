using DV8.Console.Services;
using Microsoft.Extensions.DependencyInjection;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<FirmwareManager>();
builder.Services.AddSingleton<AnalyticsService>();
builder.Services.AddSingleton<SandboxManager>();
builder.Services.AddSingleton<ComplianceService>();
builder.Services.AddSingleton<VulnWatchService>();
builder.Services.AddSingleton<PathTracerService>();
builder.Services.AddSingleton<AutoHealerService>();
builder.Services.AddSingleton<GuardRailService>();
builder.Services.AddSingleton<SyntheticsService>();
builder.Services.AddSingleton<PlannerService>();
builder.Services.AddSingleton<QuantumShieldService>();
builder.Services.AddSingleton<SelfHealService>();
builder.Services.AddSingleton<DiscoveryService>();
builder.Services.AddSingleton<ZeroTouchService>();

var app = builder.Build();

app.UseMiddleware<DV8.Console.Middleware.ErrorHandlingMiddleware>();
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("X-DV8-Decision"))
        ctx.Response.Headers.Append("X-DV8-Decision", "ALLOW");
});

using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider.GetRequiredService<FirmwareManager>();
    scope.ServiceProvider.GetRequiredService<AnalyticsService>();
    scope.ServiceProvider.GetRequiredService<SandboxManager>();
    scope.ServiceProvider.GetRequiredService<ComplianceService>();
    scope.ServiceProvider.GetRequiredService<VulnWatchService>();
    scope.ServiceProvider.GetRequiredService<PathTracerService>();
    scope.ServiceProvider.GetRequiredService<AutoHealerService>();
    scope.ServiceProvider.GetRequiredService<GuardRailService>();
    scope.ServiceProvider.GetRequiredService<SyntheticsService>();
    scope.ServiceProvider.GetRequiredService<PlannerService>();
    scope.ServiceProvider.GetRequiredService<QuantumShieldService>();
    scope.ServiceProvider.GetRequiredService<SelfHealService>();
    scope.ServiceProvider.GetRequiredService<DiscoveryService>();
    scope.ServiceProvider.GetRequiredService<ZeroTouchService>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program { }

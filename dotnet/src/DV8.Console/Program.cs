using DV8.Console.Services;
using DV8.Console.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Entity Framework
builder.Services.AddDbContext<DV8DbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register database-enabled services
builder.Services.AddScoped<DeviceService>();
builder.Services.AddScoped<DiscoveryService>();

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
builder.Services.AddSingleton<ZeroTouchService>();

var app = builder.Build();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DV8DbContext>();
    try
    {
        context.Database.EnsureCreated();
    }
    catch (Microsoft.Data.Sqlite.SqliteException ex) when (ex.Message.Contains("already exists"))
    {
        // Database already exists, this is fine - likely created by Python component
        Console.WriteLine("Database already exists, skipping creation.");
    }
    
    // Initialize services that don't require database context
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
    scope.ServiceProvider.GetRequiredService<ZeroTouchService>();
    
    // Initialize database-dependent services
    scope.ServiceProvider.GetRequiredService<DeviceService>();
    scope.ServiceProvider.GetRequiredService<DiscoveryService>();
}

app.UseMiddleware<DV8.Console.Middleware.ErrorHandlingMiddleware>();
app.Use(async (ctx, next) =>
{
    await next();
    if (!ctx.Response.Headers.ContainsKey("X-DV8-Decision"))
        ctx.Response.Headers["X-DV8-Decision"] = "ALLOW";
});

using (var scope = app.Services.CreateScope())
{
    // Initialize services that don't require database context
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
    scope.ServiceProvider.GetRequiredService<ZeroTouchService>();
    
    // Initialize database-dependent services
    scope.ServiceProvider.GetRequiredService<DeviceService>();
    scope.ServiceProvider.GetRequiredService<DiscoveryService>();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.Run();

public partial class Program { }

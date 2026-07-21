using Serilog;
using Warehouse.Api.Endpoints.Products;
using Warehouse.Api.Endpoints.Warehouses;
using Warehouse.Api.Middleware;
using Warehouse.Application;
using Warehouse.Infrastructure;
using Warehouse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext());

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services
    .AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<WarehouseDbContext>("postgresql", tags: ["ready"]);

var app = builder.Build();

app.UseSerilogRequestLogging();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new { service = "Warehouse API", status = "ready" }));
app.MapProductEndpoints();
app.MapWarehouseEndpoints();
app.MapHealthChecks("/health/live", new()
{
    Predicate = registration => registration.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new()
{
    Predicate = registration => registration.Tags.Contains("ready")
});

app.Run();

public partial class Program;
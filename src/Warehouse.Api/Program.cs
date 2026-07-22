using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using Warehouse.Api.Auth;
using Warehouse.Api.Auditing;
using Warehouse.Api.Identity;
using Warehouse.Api.Endpoints.Administration;
using Warehouse.Api.Endpoints.Auth;
using Warehouse.Api.Endpoints.Products;
using Warehouse.Api.Endpoints.Warehouses;
using Warehouse.Api.Middleware;
using Warehouse.Application;
using Warehouse.Infrastructure;
using Warehouse.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetRequiredSection(JwtOptions.SectionName).Get<JwtOptions>()
    ?? throw new InvalidOperationException("JWT configuration is required.");
jwt.Validate();
var frontendOrigin = builder.Configuration["Frontend:Origin"];

builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration).ReadFrom.Services(services).Enrich.FromLogContext());
builder.Services.Configure<JwtOptions>(builder.Configuration.GetRequiredSection(JwtOptions.SectionName));
builder.Services.AddApplication();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Warehouse.Application.Common.Identity.ICurrentUser, HttpCurrentUser>();
builder.Services.AddScoped<Warehouse.Application.Common.Auditing.IAuditContext, HttpContextAuditContext>();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<JwtTokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters { ValidateIssuer = true, ValidIssuer = jwt.Issuer, ValidateAudience = true, ValidAudience = jwt.Audience, ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SigningKey)), ValidateLifetime = true, ClockSkew = TimeSpan.FromMinutes(1) });
builder.Services.AddAuthorization(options => { options.AddPolicy(AuthorizationPolicies.ReadCatalog, policy => policy.RequireRole(AuthorizationPolicies.AdminRole, AuthorizationPolicies.ManagerRole, AuthorizationPolicies.OperatorRole)); options.AddPolicy(AuthorizationPolicies.ManageCatalog, policy => policy.RequireRole(AuthorizationPolicies.AdminRole, AuthorizationPolicies.ManagerRole)); options.AddPolicy(AuthorizationPolicies.ManageAdministration, policy => policy.RequireRole(AuthorizationPolicies.AdminRole)); });
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, AuthorizationProblemDetailsMiddlewareResultHandler>();
if (!string.IsNullOrWhiteSpace(frontendOrigin)) builder.Services.AddCors(options => options.AddPolicy("frontend", policy => policy.WithOrigins(frontendOrigin).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));
builder.Services.AddProblemDetails(); builder.Services.AddExceptionHandler<UnexpectedExceptionHandler>(); builder.Services.AddOpenApi(); builder.Services.AddSwaggerGen(options => options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme { Type = SecuritySchemeType.Http, Scheme = "bearer", BearerFormat = "JWT", Description = "Enter a JWT access token." }));
builder.Services.AddHealthChecks().AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"]).AddDbContextCheck<WarehouseDbContext>("postgresql", tags: ["ready"]);
var app = builder.Build();
if (app.Environment.IsDevelopment()) { using var scope = app.Services.CreateScope(); await scope.ServiceProvider.GetRequiredService<Warehouse.Infrastructure.Identity.IdentityBootstrapper>().SeedDevelopmentAdminAsync(); }
app.MapGet("/", () => Results.Ok(new { service = "Warehouse API", status = "ready" }));
app.MapAuthEndpoints();
app.MapAdministrationEndpoints();
app.MapProductEndpoints();
app.MapWarehouseEndpoints();
app.MapHealthChecks("/health/live", new() { Predicate = registration => registration.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = registration => registration.Tags.Contains("ready") });
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
if (!string.IsNullOrWhiteSpace(frontendOrigin)) app.UseCors("frontend");
app.UseRouting();
app.UseAuthentication();
app.UseMiddleware<CatalogAuthorizationMiddleware>();
app.UseAuthorization();
if (app.Environment.IsDevelopment()) { app.MapOpenApi(); app.UseSwagger(); app.UseSwaggerUI(); }
app.Run();
public partial class Program;

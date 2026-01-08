using Budget.Api.Auth;
using Budget.Api.Middleware;
using Budget.Core;
using Budget.Core.Interfaces;
using Budget.Infrastructure;
using Budget.Infrastructure.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/budget-api-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddCore();
builder.Services.AddInfrastructure(builder.Configuration);

// Authentication
var useDevAuth = builder.Configuration.GetValue<bool>("Auth:UseDevAuth");

if (useDevAuth)
{
    // Development: simple dev auth
    builder.Services.AddAuthentication("DevAuth")
        .AddScheme<DevAuthOptions, DevAuthHandler>("DevAuth", options =>
        {
            options.DevUserId = builder.Configuration["Auth:DevUserId"] ?? "dev-user";
            options.DevUserName = builder.Configuration["Auth:DevUserName"] ?? "Developer";
            options.DevUserEmail = builder.Configuration["Auth:DevUserEmail"] ?? "dev@localhost";
        });
}
else
{
    // Production: Entra ID (Azure AD) authentication
    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));
}

builder.Services.AddAuthorization();

// Current user service
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUser, HttpContextCurrentUser>();

// Controllers
builder.Services.AddControllers();

// FluentValidation auto-validation in MVC
builder.Services.AddValidatorsFromAssemblyContaining<Budget.Core.Application.Validators.UploadFileCommandValidator>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Budget Platform API",
        Version = "v1",
        Description = "Enterprise Budget Management Platform"
    });

    // JWT Bearer authentication in Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<BudgetDbContext>("database");

// CORS for Teams Tab
builder.Services.AddCors(options =>
{
    options.AddPolicy("TeamsPolicy", policy =>
    {
        policy.WithOrigins(
                builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "https://teams.microsoft.com" })
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

var app = builder.Build();

// Global exception handling
app.UseMiddleware<ExceptionMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Budget Platform API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("TeamsPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Serilog request logging
app.UseSerilogRequestLogging();

// Health endpoints
app.MapHealthChecks("/health");
app.MapHealthChecks("/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

// Teams Tab route
app.MapGet("/teams", () => Results.Ok(new { status = "Teams Tab Ready", timestamp = DateTime.UtcNow }))
    .WithName("TeamsTab")
    .WithOpenApi();

app.MapControllers();

// Apply migrations in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BudgetDbContext>();
    await db.Database.MigrateAsync();
}

Log.Information("Budget Platform API starting...");
await app.RunAsync();

// Make Program class accessible for integration tests
public partial class Program { }


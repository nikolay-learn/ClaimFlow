using System.ComponentModel.DataAnnotations;
using ClaimFlow.Api.Data;
using ClaimFlow.Api.Domain;
using ClaimFlow.Api.Features.Claims;
using ClaimFlow.Api.Infrastructure;
using Scalar.AspNetCore;

Console.WriteLine("Test CI");

var builder = WebApplication.CreateBuilder(args);
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
    options.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter())
);
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddValidation();
builder.Services.AddExceptionHandler<ClaimRuleExceptionHandler>();
builder.Services.AddSingleton<IClaimRepository, InMemoryClaimRepository>();

builder.Services.AddOptions<ClaimRulesOptions>().BindConfiguration(ClaimRulesOptions.SectionName).ValidateDataAnnotations().ValidateOnStart();

var app = builder.Build();

app.UseMiddleware<RequestTimingMiddleware>();
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

    app.MapGet("/debug/config-sources", (IConfiguration config) => ((IConfigurationRoot)config).Providers.Select(p => p.ToString()).ToList());
}

if (!string.IsNullOrWhiteSpace(app.Configuration.GetConnectionString("ClaimsDb")))
{
    app.Logger.ClaimdDbConfigured();
}

app.MapGet("/", () => "ClaimFlow API is running");
app.MapGet("/health", () => Results.Ok(new { Status = "Healthy", CheckedAt = DateTimeOffset.UtcNow }));
app.MapGet(
    "/version",
    () =>
        Results.Ok(
            new
            {
                name = "ClaimFlow",
                version = "0.1.0",
                environment = app.Environment.EnvironmentName,
            }
        )
);

app.MapGet(
    "/payout",
    ([Range(0, double.MaxValue)] decimal amount, [Range(0, double.MaxValue)] decimal deductible, [Range(0, double.MaxValue)] decimal limit) =>
    {
        var payout = PayoutCalculator.Calculate(amount, deductible, limit);
        return new
        {
            amount,
            deductible,
            limit,
            payout,
            size = PayoutCalculator.Classify(amount),
        };
    }
);

app.MapGet(
    "/debug/boom",
    () =>
    {
        throw new InvalidOperationException("Database password is hunter2");
    }
);

app.MapClaimEndpoints();

app.Run();

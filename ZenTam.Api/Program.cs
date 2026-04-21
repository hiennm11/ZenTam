using FluentValidation;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ZenTamDbContext>(opt =>
    opt.UseInMemoryDatabase("ZenTamDb"));

// ── Lunar calculator ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ILunarCalculatorService, AmLichCalculator>();

// ── Rule engine ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<RuleResolver>();
builder.Services.AddSingleton<ISpiritualRule, KimLauRule>();
builder.Services.AddSingleton<ISpiritualRule, HoangOcRule>();
builder.Services.AddSingleton<ISpiritualRule, TamTaiRule>();

// ── Feature handlers ──────────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateActionValidator>();

// ── Middleware ────────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Seed data ─────────────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ZenTamDbContext>();
    await DataSeeder.SeedAsync(db);
}

// ── Pipeline ──────────────────────────────────────────────────────────────────
app.UseExceptionHandler();
app.MapOpenApi();
app.MapScalarApiReference();

EvaluateActionEndpoint.Map(app);

app.Run();

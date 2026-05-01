using FluentValidation;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using StackExchange.Redis;
using ZenTam.Api.Common.CanChi;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Domain.Services;
using ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction.Queries;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Features.ParseAndEvaluate.Queries;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.Features.Clients.Queries;
using ZenTam.Api.Features.Calendars;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ZenTamDbContext>(opt =>
    opt.UseInMemoryDatabase("ZenTamDb"));

// ── Lunar calculator ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ILunarCalculatorService, AmLichCalculator>();

// ── Solar Term Calculator ─────────────────────────────────────────────────────
builder.Services.AddScoped<ISolarTermCalculator, SolarTermCalculator>();

// ── Can Chi Calculator ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICanChiCalculator, CanChiCalculator>();

// ── Gánh Mệnh Service ────────────────────────────────────────────────────────
builder.Services.AddScoped<IGanhMenhService, GanhMenhService>();

// ── Rule engine ───────────────────────────────────────────────────────────────
builder.Services.AddScoped<RuleResolver>();
builder.Services.AddSingleton<ISpiritualRule, KimLauRule>();
builder.Services.AddSingleton<ISpiritualRule, HoangOcRule>();
builder.Services.AddSingleton<ISpiritualRule, TamTaiRule>();
builder.Services.AddSingleton<ISpiritualRule, ThaiTueRule>();
builder.Services.AddSingleton<ISpiritualRule, NguyetKyRule>();
builder.Services.AddSingleton<ISpiritualRule, TamNuongRule>();
builder.Services.AddSingleton<ISpiritualRule, XungTuoiRule>();
builder.Services.AddSingleton<ISpiritualRule>(sp => 
    new TamSatThangRule(sp.GetRequiredService<ICanChiCalculator>()));

builder.Services.AddSingleton<IMonthlyRuleEngine, MonthlyRuleEngine>();

// ── Feature handlers ──────────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateActionValidator>();

// ── FindGoodDays services ───────────────────────────────────────────────────
builder.Services.AddScoped<IDayScoreCalculator, DayScoreCalculator>();
builder.Services.AddScoped<IFindGoodDaysService, FindGoodDaysService>();

// ── Client feature handlers ──────────────────────────────────────────────────
builder.Services.AddScoped<CreateClientHandler>();
builder.Services.AddScoped<SearchClientsHandler>();
builder.Services.AddScoped<GetClientHandler>();
builder.Services.AddScoped<AddRelatedPersonHandler>();
builder.Services.AddScoped<DeleteRelatedPersonHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientValidator>();

// ── Day Context & Hoàng Đạo ───────────────────────────────────────────────────
builder.Services.AddScoped<IDayContextService, DayContextService>();
builder.Services.AddScoped<IHoangDaoService, HoangDaoService>();

// ── Cache ─────────────────────────────────────────────────────────────────────
string? redisConnectionString = builder.Configuration.GetConnectionString("Redis");
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(
        ConnectionMultiplexer.Connect(redisConnectionString));
    builder.Services.AddSingleton<ICacheService, RedisCacheService>();
}
else
{
    builder.Services.AddMemoryCache();
    builder.Services.AddSingleton<ICacheService, MemoryCacheService>();
}

// ── HTTP Clients ──────────────────────────────────────────────────────────────
builder.Services.AddHttpClient("LiteLLM", c =>
{
    c.BaseAddress = new Uri("https://llm.hienlab.com/");
    c.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        "Bearer",
        "sk-fT0tCFdMmgrcmpR50tcYSA");
    c.Timeout = TimeSpan.FromSeconds(10);
});

// ── Intent Parsers ────────────────────────────────────────────────────────────
builder.Services.AddScoped<RegexIntentParser>();
builder.Services.AddScoped<SLMIntentParser>();
builder.Services.AddScoped<ParseAndEvaluateHandler>();

// ── ActionCode Mapper for DB-Enum sync ───────────────────────────────────────
builder.Services.AddScoped<ActionCodeMapper>();

// ── Middleware ────────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddControllers();
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
app.MapControllers();

FindGoodDaysEndpoint.MapFindGoodDays(app);
EvaluateActionEndpoint.Map(app);
ParseAndEvaluateEndpoint.Map(app);

// ── Client CRUD endpoints ────────────────────────────────────────────────────
CreateClientEndpoint.Map(app);
SearchClientsEndpoint.Map(app);
GetClientEndpoint.Map(app);
AddRelatedPersonEndpoint.Map(app);
DeleteRelatedPersonEndpoint.Map(app);

app.Run();

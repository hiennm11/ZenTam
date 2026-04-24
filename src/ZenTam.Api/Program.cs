using FluentValidation;
using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using StackExchange.Redis;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Features.ParseAndEvaluate.Queries.IntentParsing;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.Rules;
using ZenTam.Api.Features.EvaluateSpiritualAction.Queries;
using ZenTam.Api.Features.EvaluateSpiritualAction.Rules;
using ZenTam.Api.Features.ParseAndEvaluate.Queries;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.Features.Clients.Queries;

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
builder.Services.AddSingleton<ISpiritualRule, ThaiTueRule>();

// ── Feature handlers ──────────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateActionValidator>();

// ── Client feature handlers ──────────────────────────────────────────────────
builder.Services.AddScoped<CreateClientHandler>();
builder.Services.AddScoped<SearchClientsHandler>();
builder.Services.AddScoped<GetClientHandler>();
builder.Services.AddScoped<AddRelatedPersonHandler>();
builder.Services.AddScoped<DeleteRelatedPersonHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientValidator>();

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
ParseAndEvaluateEndpoint.Map(app);

// ── Client CRUD endpoints ────────────────────────────────────────────────────
CreateClientEndpoint.Map(app);
SearchClientsEndpoint.Map(app);
GetClientEndpoint.Map(app);
AddRelatedPersonEndpoint.Map(app);
DeleteRelatedPersonEndpoint.Map(app);

app.Run();

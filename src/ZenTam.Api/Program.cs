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
using ZenTam.Api.Features.EvaluateSpiritualAction.Handlers;
using ZenTam.Api.Features.ParseAndEvaluate.Queries;
using EvaluateSpiritualActionEndpoints = ZenTam.Api.Features.EvaluateSpiritualAction.Endpoints;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Features.Clients.Commands;
using ZenTam.Api.Features.Clients.Queries;
using ZenTam.Api.Features.Calendars;
using ZenTam.Api.Features.Calendars.Services;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine;
using ZenTam.Api.Domain.Rules.MonthlyRuleEngine.Rules;

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
builder.Services.AddScoped<RuleResolverV2>();

// Register each rule as both ISpiritualRule AND its concrete type
// so MonthlyRuleEngine can resolve them directly by type
builder.Services.AddSingleton<ISpiritualRule>(sp => new NguyetKyRuleV2());
builder.Services.AddSingleton<NguyetKyRuleV2>(sp => sp.GetRequiredService<IEnumerable<ISpiritualRule>>().OfType<NguyetKyRuleV2>().First());

builder.Services.AddSingleton<ISpiritualRule>(sp => new TamNuongRuleV2());
builder.Services.AddSingleton<TamNuongRuleV2>(sp => sp.GetRequiredService<IEnumerable<ISpiritualRule>>().OfType<TamNuongRuleV2>().First());

builder.Services.AddSingleton<ISpiritualRule>(sp => new XungTuoiRuleV2());
builder.Services.AddSingleton<XungTuoiRuleV2>(sp => sp.GetRequiredService<IEnumerable<ISpiritualRule>>().OfType<XungTuoiRuleV2>().First());

builder.Services.AddSingleton<ISpiritualRule>(sp => new SatChuRuleV2());
builder.Services.AddSingleton<SatChuRuleV2>(sp => sp.GetRequiredService<IEnumerable<ISpiritualRule>>().OfType<SatChuRuleV2>().First());

builder.Services.AddSingleton<IMonthlyRuleEngine>(sp =>
{
    var nguyetKy = sp.GetRequiredService<NguyetKyRuleV2>();
    var tamNuong = sp.GetRequiredService<TamNuongRuleV2>();
    var xungTuoi = sp.GetRequiredService<XungTuoiRuleV2>();
    var satChu = sp.GetRequiredService<SatChuRuleV2>();
    var duongCongKy = new DuongCongKyRuleV2(
        sp.GetRequiredService<ILunarCalculatorService>(),
        sp.GetRequiredService<ICanChiCalculator>());
    return new MonthlyRuleEngine([nguyetKy, tamNuong, xungTuoi, satChu, duongCongKy]);
});

// ── Feature handlers ──────────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionHandler>();
builder.Services.AddValidatorsFromAssemblyContaining<EvaluateActionValidator>();

// ── Year / Month / Day tier handlers ─────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionWrapperHandler>();
builder.Services.AddScoped<EvaluateActionYearHandler>();
builder.Services.AddScoped<EvaluateActionMonthHandler>();
builder.Services.AddScoped<EvaluateActionDayHandler>();

// ── FindGoodDays services ───────────────────────────────────────────────────
builder.Services.AddScoped<IDayScoreCalculator, DayScoreCalculator>();
builder.Services.AddScoped<IFindGoodDaysService, FindGoodDaysService>();

// ── Day-tier evaluation ──────────────────────────────────────────────────────
builder.Services.AddScoped<EvaluateActionDailyHandler>();
builder.Services.AddScoped<IValidator<EvaluateActionDailyRequest>, EvaluateActionDailyValidator>();

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
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
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
EvaluateActionEndpoint.Map(app);
EvaluateActionDailyEndpoint.Map(app);
ParseAndEvaluateEndpoint.Map(app);
EvaluateSpiritualActionEndpoints.EvaluateActionYearEndpoint.Map(app);
EvaluateSpiritualActionEndpoints.EvaluateActionMonthEndpoint.Map(app);
EvaluateSpiritualActionEndpoints.EvaluateActionDayEndpoint.Map(app);

FindGoodDaysEndpoint.MapFindGoodDays(app);

// ── Client CRUD endpoints ────────────────────────────────────────────────────
CreateClientEndpoint.Map(app);
SearchClientsEndpoint.Map(app);
GetClientEndpoint.Map(app);
AddRelatedPersonEndpoint.Map(app);
DeleteRelatedPersonEndpoint.Map(app);

app.Run();

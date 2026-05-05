using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Caching;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Common.CanChi;

var builder = WebApplication.CreateBuilder(args);

// ── Database ──────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<ZenTamDbContext>(opt =>
    opt.UseInMemoryDatabase("ZenTamDb"));

// ── Lunar calculator ──────────────────────────────────────────────────────────
builder.Services.AddScoped<ILunarCalculatorService, AmLichCalculator>();

// ── Can Chi Calculator ────────────────────────────────────────────────────────
builder.Services.AddScoped<ICanChiCalculator, CanChiCalculator>();

// ── Cache ─────────────────────────────────────────────────────────────────────
builder.Services.AddMemoryCache();
builder.Services.AddSingleton<ICacheService, MemoryCacheService>();

// ── Middleware ────────────────────────────────────────────────────────────────
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
builder.Services.AddOpenApi();

var app = builder.Build();

app.UseExceptionHandler();
app.MapOpenApi();
app.MapControllers();

app.Run();

// Temporary placeholder for DbContext - will be rebuilt with core engine
public class ZenTamDbContext : DbContext
{
    public ZenTamDbContext(DbContextOptions<ZenTamDbContext> options) : base(options) { }
}

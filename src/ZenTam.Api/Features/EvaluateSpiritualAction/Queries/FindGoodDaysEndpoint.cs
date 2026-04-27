using System.Text.Json;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Features.EvaluateSpiritualAction.Services;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public static class FindGoodDaysEndpoint
{
    public static void MapFindGoodDays(this IEndpointRouteBuilder app)
    {
        app.MapPost("/api/evaluate/find-good-days", async (
            FindGoodDaysRequest request,
            IFindGoodDaysService service,
            CancellationToken ct) =>
        {
            // Validate date range
            if (request.FromDate > request.ToDate)
            {
                return Results.BadRequest(new { error = "FromDate must be before or equal to ToDate" });
            }

            // Limit search to 365 days to prevent abuse
            var daysSpan = request.ToDate.DayNumber - request.FromDate.DayNumber;
            if (daysSpan > 365)
            {
                return Results.BadRequest(new { error = "Date range cannot exceed 365 days" });
            }

            var result = await service.FindGoodDaysAsync(request, ct);
            return Results.Ok(result);
        });

        // SSE Streaming endpoint
        app.MapGet("/api/evaluate/find-good-days/stream", async (
            Guid clientId,
            ActionCode action,
            DateOnly fromDate,
            DateOnly toDate,
            Guid? subjectClientId,
            int maxResults,
            IFindGoodDaysService service,
            HttpContext httpContext,
            CancellationToken ct) =>
        {
            // Validate date range
            if (fromDate > toDate)
            {
                return Results.BadRequest(new { error = "FromDate must be before or equal to ToDate" });
            }

            // Limit search to 365 days to prevent abuse
            var daysSpan = toDate.DayNumber - fromDate.DayNumber;
            if (daysSpan > 365)
            {
                return Results.BadRequest(new { error = "Date range cannot exceed 365 days" });
            }

            var request = new FindGoodDaysRequest(
                ClientId: clientId,
                Action: action,
                FromDate: fromDate,
                ToDate: toDate,
                SubjectClientId: subjectClientId,
                MaxResults: maxResults
            );

            var totalDays = daysSpan + 1;
            var progress = 0;

            httpContext.Response.ContentType = "text/event-stream";
            httpContext.Response.Headers.Append("Cache-Control", "no-cache");
            httpContext.Response.Headers.Append("X-Accel-Buffering", "no");

            await foreach (var result in service.StreamFindGoodDaysAsync(request, ct))
            {
                progress++;
                var percent = (progress * 100) / totalDays;

                var sseData = new
                {
                    progress,
                    total = totalDays,
                    percent,
                    date = result.SolarDate.ToString("yyyy-MM-dd"),
                    score = result.Score,
                    isGood = result.Score >= 60
                };

                var json = JsonSerializer.Serialize(sseData);
                await httpContext.Response.WriteAsync($"data: {json}\n\n", ct);
            }

            return Results.Ok();
        });
    }
}
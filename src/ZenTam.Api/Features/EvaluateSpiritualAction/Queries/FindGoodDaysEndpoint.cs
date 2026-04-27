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
    }
}
using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Lunar;
using ZenTam.Api.Features.EvaluateSpiritualAction.Models;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Services;

public class FindGoodDaysService(
    ZenTamDbContext db,
    IDayScoreCalculator scoreCalculator,
    ILunarCalculatorService lunarCalculator
) : IFindGoodDaysService
{
    public async Task<FindGoodDaysResponse> FindGoodDaysAsync(
        FindGoodDaysRequest request,
        CancellationToken ct = default)
    {
        int? subjectLunarYear = null;

        // Step 1: Load subject client's lunar year for xung tuổi check
        if (request.SubjectClientId.HasValue)
        {
            var subject = await db.ClientProfiles
                .FirstOrDefaultAsync(c => c.Id == request.SubjectClientId.Value, ct);

            if (subject != null)
            {
                var lunar = lunarCalculator.Convert(subject.SolarDob);
                subjectLunarYear = lunar.LunarYear;
            }
        }

        // Step 2: Iterate through date range
        var results = new List<DayScoreResult>();
        var current = request.FromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = request.ToDate.ToDateTime(TimeOnly.MinValue);

        while (current <= endDate)
        {
            var scoreResult = scoreCalculator.Calculate(current, request.Action, subjectLunarYear);
            results.Add(scoreResult);
            current = current.AddDays(1);
        }

        // Step 3: Sort by score descending and take top N
        var topResults = results
            .OrderByDescending(r => r.Score)
            .ThenBy(r => r.SolarDate)
            .Take(request.MaxResults)
            .ToList();

        return new FindGoodDaysResponse(
            Action: request.Action,
            SearchRangeStart: request.FromDate,
            SearchRangeEnd: request.ToDate,
            TotalDaysScanned: results.Count,
            SuggestedDays: topResults
        );
    }
}
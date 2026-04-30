using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Domain;
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
        Gender userGender = Gender.Male; // default

        // Step 1: Load subject client's lunar year for xung tuổi check and user's gender
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

        // Step 1b: Load client's gender for rule filtering
        var client = await db.ClientProfiles.FirstOrDefaultAsync(c => c.Id == request.ClientId, ct);
        if (client != null)
        {
            userGender = client.Gender;
        }

        // Step 2: Iterate through date range (using Day tier for daily evaluation)
        var results = new List<DayScoreResult>();
        var current = request.FromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = request.ToDate.ToDateTime(TimeOnly.MinValue);

        while (current <= endDate)
        {
            var scoreResult = scoreCalculator.Calculate(current, request.Action, userGender, RuleTier.Day, subjectLunarYear);
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

    public async IAsyncEnumerable<DayScoreResult> StreamFindGoodDaysAsync(
        FindGoodDaysRequest request,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken ct = default)
    {
        int? subjectLunarYear = null;
        Gender userGender = Gender.Male; // default

        // Step 1: Load subject client's lunar year for xung tuổi check and user's gender
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

        // Step 1b: Load client's gender for rule filtering
        var client = await db.ClientProfiles.FirstOrDefaultAsync(c => c.Id == request.ClientId, ct);
        if (client != null)
        {
            userGender = client.Gender;
        }

        // Step 2: Iterate through date range and yield each result (using Day tier)
        var current = request.FromDate.ToDateTime(TimeOnly.MinValue);
        var endDate = request.ToDate.ToDateTime(TimeOnly.MinValue);

        while (current <= endDate)
        {
            ct.ThrowIfCancellationRequested();

            var scoreResult = scoreCalculator.Calculate(current, request.Action, userGender, RuleTier.Day, subjectLunarYear);
            yield return scoreResult;

            current = current.AddDays(1);

            // Small delay to prevent CPU spinning (can be removed for max performance)
            await Task.Delay(1, ct);
        }
    }
}
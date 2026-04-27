using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Services;

public interface IFindGoodDaysService
{
    Task<FindGoodDaysResponse> FindGoodDaysAsync(
        FindGoodDaysRequest request,
        CancellationToken ct = default);
}
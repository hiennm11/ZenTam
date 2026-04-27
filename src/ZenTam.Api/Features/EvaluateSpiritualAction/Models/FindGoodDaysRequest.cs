using ZenTam.Api.Features.EvaluateSpiritualAction.Models;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Models;

public record FindGoodDaysRequest(
    Guid ClientId,
    ActionCode Action,
    DateOnly FromDate,
    DateOnly ToDate,
    Guid? SubjectClientId = null,
    int MaxResults = 5
);
namespace ZenTam.Api.Features.EvaluateSpiritualAction.Handlers;

using ZenTam.Api.Features.EvaluateSpiritualAction.Requests;
using ZenTam.Api.Features.EvaluateSpiritualAction.Responses;

public class EvaluateActionWrapperHandler
{
    private readonly EvaluateActionYearHandler  _yearHandler;
    private readonly EvaluateActionMonthHandler _monthHandler;
    private readonly EvaluateActionDayHandler  _dayHandler;

    public EvaluateActionWrapperHandler(
        EvaluateActionYearHandler  yearHandler,
        EvaluateActionMonthHandler monthHandler,
        EvaluateActionDayHandler  dayHandler)
    {
        _yearHandler  = yearHandler;
        _monthHandler = monthHandler;
        _dayHandler   = dayHandler;
    }

    public async Task<EvaluateActionResponse> HandleAsync(
        EvaluateActionRequest request,
        CancellationToken ct = default)
    {
        if (request.TargetDate.HasValue)
        {
            return await _dayHandler.HandleAsync(
                new EvaluateActionDayRequest
                {
                    UserId      = request.UserId,
                    ActionCode  = request.ActionCode,
                    TargetDate  = request.TargetDate.Value
                }, ct);
        }

        if (request.TargetMonth.HasValue)
        {
            return await _monthHandler.HandleAsync(
                new EvaluateActionMonthRequest
                {
                    UserId      = request.UserId,
                    ActionCode  = request.ActionCode,
                    TargetYear  = request.TargetYear ?? DateTime.Now.Year,
                    TargetMonth = request.TargetMonth.Value,
                    TargetDay   = request.TargetDay ?? 1
                }, ct);
        }

        return await _yearHandler.HandleAsync(
            new EvaluateActionYearRequest
            {
                UserId     = request.UserId,
                ActionCode = request.ActionCode,
                TargetYear = request.TargetYear ?? DateTime.Now.Year
            }, ct);
    }
}
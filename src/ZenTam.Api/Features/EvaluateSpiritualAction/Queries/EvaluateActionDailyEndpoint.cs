using FluentValidation;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

/// <summary>
/// Endpoint for evaluating spiritual actions on a specific day (Day-tier).
/// Route: POST /api/evaluate/action/daily
/// </summary>
public static class EvaluateActionDailyEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/evaluate/action/daily", async (
            EvaluateActionDailyRequest request,
            IValidator<EvaluateActionDailyRequest> validator,
            EvaluateActionDailyHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                // Validate
                var validationResult = await validator.ValidateAsync(request, ct);
                if (!validationResult.IsValid)
                {
                    var errors = validationResult.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());
                    return Results.Problem(
                        title: "Validation Failed",
                        statusCode: 400,
                        extensions: new Dictionary<string, object?> { ["errors"] = errors });
                }

                // Handle
                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(response);
            }
            catch (BadHttpRequestException ex)
            {
                return Results.Problem(
                    title: "Bad Request",
                    detail: ex.Message,
                    statusCode: 400);
            }
            catch (NotFoundException ex)
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: ex.Message,
                    statusCode: 404);
            }
            catch (Exception ex)
            {
                return Results.Problem(
                    title: "Internal Server Error",
                    detail: ex.Message,
                    statusCode: 500);
            }
        })
        .WithName("EvaluateSpiritualActionDaily")
        .WithOpenApi();
    }
}
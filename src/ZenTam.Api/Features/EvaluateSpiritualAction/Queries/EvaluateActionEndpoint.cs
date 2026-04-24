using FluentValidation;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.EvaluateSpiritualAction.Queries;

public static class EvaluateActionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/evaluate", async (
            EvaluateActionRequest          request,
            IValidator<EvaluateActionRequest> validator,
            EvaluateActionHandler          handler,
            CancellationToken              ct) =>
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
            try
            {
                var response = await handler.HandleAsync(request, ct);
                return Results.Ok(response);
            }
            catch (NotFoundException ex)
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: ex.Message,
                    statusCode: 404);
            }
        })
        .WithName("EvaluateSpiritualAction")
        .WithOpenApi();
    }
}


using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.ParseAndEvaluate;

public static class ParseAndEvaluateEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/chat", async (
            ParseAndEvaluateRequest             request,
            IValidator<ParseAndEvaluateRequest> validator,
            ParseAndEvaluateHandler             handler,
            CancellationToken                   ct) =>
        {
            var validation = await validator.ValidateAsync(request, ct);
            if (!validation.IsValid)
                return Results.ValidationProblem(validation.ToDictionary());

            try
            {
                var result = await handler.HandleAsync(request, ct);
                return Results.Ok(result);
            }
            catch (NotFoundException ex)
            {
                return Results.Problem(
                    detail:     ex.Message,
                    statusCode: StatusCodes.Status404NotFound);
            }
            catch (UnprocessableEntityException ex)
            {
                return Results.Problem(
                    detail:     ex.Message,
                    statusCode: StatusCodes.Status422UnprocessableEntity);
            }
        })
        .WithName("ParseAndEvaluate")
        .WithOpenApi();
    }
}

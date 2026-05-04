using FluentValidation;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.ParseAndEvaluate.Queries;

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
            try
            {
                var validation = await validator.ValidateAsync(request, ct);
                if (!validation.IsValid)
                {
                    var errors = validation.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(
                            g => g.Key,
                            g => g.Select(e => e.ErrorMessage).ToArray());
                    return Results.Problem(
                        title: "Validation Failed",
                        statusCode: 400,
                        extensions: new Dictionary<string, object?> { ["errors"] = errors });
                }

                var result = await handler.HandleAsync(request, ct);
                return Results.Ok(result);
            }
            catch (BadHttpRequestException ex)
            {
                return Results.Problem(
                    title: "Bad Request",
                    detail: ex.Message,
                    statusCode: StatusCodes.Status400BadRequest);
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
            catch (Exception ex)
            {
                return Results.Problem(
                    detail:     ex.Message,
                    statusCode: StatusCodes.Status500InternalServerError);
            }
        })
        .WithName("ParseAndEvaluate")
        .WithOpenApi();
    }
}

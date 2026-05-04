namespace ZenTam.Api.Features.EvaluateSpiritualAction.Endpoints;

using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Features.EvaluateSpiritualAction.Handlers;
using ZenTam.Api.Features.EvaluateSpiritualAction.Requests;

public static class EvaluateActionEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/evaluate/action", async (
            EvaluateActionRequest request,
            EvaluateActionWrapperHandler handler,
            CancellationToken ct) =>
        {
            try
            {
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
                return Results.Problem(title: "Not Found", detail: ex.Message, statusCode: 404);
            }
            catch (Exception ex)
            {
                return Results.Problem(title: "Internal Server Error", detail: ex.Message, statusCode: 500);
            }
        })
        .WithTags("Evaluate")
        .WithOpenApi();
    }
}

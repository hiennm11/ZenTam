using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.Clients.Commands;

public static class DeleteRelatedPersonEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapDelete("/api/clients/{id}/related/{rid}", async (
            Guid id,
            Guid rid,
            DeleteRelatedPersonHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                await handler.HandleAsync(id, rid, ct);
                return Results.NoContent();
            }
            catch (NotFoundException ex)
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: ex.Message,
                    statusCode: 404);
            }
        })
        .WithName("DeleteRelatedPerson")
        .WithOpenApi();
    }
}

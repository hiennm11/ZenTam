using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.Clients.Queries;

public static class GetClientEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/clients/{id}", async (
            Guid id,
            GetClientHandler handler,
            CancellationToken ct) =>
        {
            try
            {
                var request = new GetClientRequest { Id = id };
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
        .WithName("GetClient")
        .WithOpenApi();
    }
}

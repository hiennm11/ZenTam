namespace ZenTam.Api.Features.Clients;

public static class SearchClientsEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/clients", async (
            string? phone,
            SearchClientsHandler handler,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return Results.Problem("Phone number is required", statusCode: 400);
            }

            if (phone.Length < 3)
            {
                return Results.Problem(
                    title: "Validation Failed",
                    detail: "Phone query must be at least 3 characters",
                    statusCode: 400);
            }

            var request = new SearchClientsRequest { Phone = phone };
            var response = await handler.HandleAsync(request, ct);
            return Results.Ok(response.Clients);
        })
        .WithName("SearchClients")
        .WithOpenApi();
    }
}

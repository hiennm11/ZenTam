using FluentValidation;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.Clients;

public static class CreateClientEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/clients", async (
            CreateClientRequest request,
            IValidator<CreateClientRequest> validator,
            CreateClientHandler handler,
            CancellationToken ct) =>
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
            return Results.Created($"/api/clients/{response.Id}", response);
        })
        .WithName("CreateClient")
        .WithOpenApi();
    }
}

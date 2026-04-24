using FluentValidation;
using ZenTam.Api.Common.Exceptions;

namespace ZenTam.Api.Features.Clients;

public static class AddRelatedPersonEndpoint
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/api/clients/{id}/related", async (
            Guid id,
            AddRelatedPersonRequest request,
            IValidator<AddRelatedPersonRequest> validator,
            AddRelatedPersonHandler handler,
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
            try
            {
                var requestWithClientId = new AddRelatedPersonRequest
                {
                    ClientId = id,
                    Label = request.Label,
                    SolarDob = request.SolarDob,
                    Gender = request.Gender
                };
                var response = await handler.HandleAsync(requestWithClientId, ct);
                return Results.Created($"/api/clients/{id}/related/{response.Id}", response);
            }
            catch (NotFoundException ex)
            {
                return Results.Problem(
                    title: "Not Found",
                    detail: ex.Message,
                    statusCode: 404);
            }
        })
        .WithName("AddRelatedPerson")
        .WithOpenApi();
    }
}

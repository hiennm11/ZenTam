using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Features.Clients.Commands;

public class CreateClientHandler
{
    private readonly ZenTamDbContext _db;

    public CreateClientHandler(ZenTamDbContext db)
    {
        _db = db;
    }

    public async Task<CreateClientResponse> HandleAsync(CreateClientRequest request, CancellationToken ct)
    {
        var client = new ClientProfile
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Username = request.Username,
            PhoneNumber = request.PhoneNumber,
            SolarDob = request.SolarDob,
            Gender = request.Gender!.Value,
            Notes = request.Notes,
            CreatedAt = DateTime.UtcNow
        };

        _db.ClientProfiles.Add(client);
        await _db.SaveChangesAsync(ct);

        return new CreateClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            Username = client.Username,
            PhoneNumber = client.PhoneNumber,
            SolarDob = client.SolarDob,
            Gender = client.Gender,
            Notes = client.Notes,
            CreatedAt = client.CreatedAt
        };
    }
}

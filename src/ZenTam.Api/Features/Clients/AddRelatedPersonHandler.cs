using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Infrastructure;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Features.Clients;

public class AddRelatedPersonHandler
{
    private readonly ZenTamDbContext _db;

    public AddRelatedPersonHandler(ZenTamDbContext db)
    {
        _db = db;
    }

    public async Task<AddRelatedPersonResponse> HandleAsync(AddRelatedPersonRequest request, CancellationToken ct)
    {
        var clientExists = await _db.ClientProfiles.AnyAsync(c => c.Id == request.ClientId, ct);
        if (!clientExists)
        {
            throw new NotFoundException($"Client {request.ClientId} not found");
        }

        var relatedPerson = new ClientRelatedPerson
        {
            Id = Guid.NewGuid(),
            ClientId = request.ClientId,
            Label = request.Label,
            SolarDob = request.SolarDob,
            Gender = request.Gender,
            CreatedAt = DateTime.UtcNow
        };

        _db.ClientRelatedPersons.Add(relatedPerson);
        await _db.SaveChangesAsync(ct);

        return new AddRelatedPersonResponse
        {
            Id = relatedPerson.Id,
            ClientId = relatedPerson.ClientId,
            Label = relatedPerson.Label,
            SolarDob = relatedPerson.SolarDob,
            Gender = relatedPerson.Gender,
            CreatedAt = relatedPerson.CreatedAt
        };
    }
}

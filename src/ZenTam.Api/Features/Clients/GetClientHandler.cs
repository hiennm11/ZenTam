using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.Clients;

public class GetClientHandler
{
    private readonly ZenTamDbContext _db;

    public GetClientHandler(ZenTamDbContext db)
    {
        _db = db;
    }

    public async Task<GetClientResponse> HandleAsync(GetClientRequest request, CancellationToken ct)
    {
        var client = await _db.ClientProfiles
            .Include(c => c.RelatedPersons)
            .FirstOrDefaultAsync(c => c.Id == request.Id, ct);

        if (client is null)
        {
            throw new NotFoundException($"Client {request.Id} not found");
        }

        return new GetClientResponse
        {
            Id = client.Id,
            Name = client.Name,
            PhoneNumber = client.PhoneNumber,
            SolarDob = client.SolarDob,
            Gender = client.Gender,
            Notes = client.Notes,
            CreatedAt = client.CreatedAt,
            RelatedPersons = client.RelatedPersons.Select(rp => new RelatedPersonDto
            {
                Id = rp.Id,
                Label = rp.Label,
                SolarDob = rp.SolarDob,
                Gender = rp.Gender
            }).ToList()
        };
    }
}

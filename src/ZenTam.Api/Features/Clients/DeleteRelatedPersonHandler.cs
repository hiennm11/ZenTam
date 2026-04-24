using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Common.Exceptions;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.Clients;

public class DeleteRelatedPersonHandler
{
    private readonly ZenTamDbContext _db;

    public DeleteRelatedPersonHandler(ZenTamDbContext db)
    {
        _db = db;
    }

    public async Task HandleAsync(Guid clientId, Guid relatedPersonId, CancellationToken ct)
    {
        var clientExists = await _db.ClientProfiles.AnyAsync(c => c.Id == clientId, ct);
        if (!clientExists)
        {
            throw new NotFoundException($"Client {clientId} not found");
        }

        var relatedPerson = await _db.ClientRelatedPersons
            .FirstOrDefaultAsync(rp => rp.Id == relatedPersonId && rp.ClientId == clientId, ct);

        if (relatedPerson is null)
        {
            throw new NotFoundException($"Related person {relatedPersonId} not found for client {clientId}");
        }

        _db.ClientRelatedPersons.Remove(relatedPerson);
        await _db.SaveChangesAsync(ct);
    }
}

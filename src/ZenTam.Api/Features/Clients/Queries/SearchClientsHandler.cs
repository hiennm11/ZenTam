using Microsoft.EntityFrameworkCore;
using ZenTam.Api.Infrastructure;

namespace ZenTam.Api.Features.Clients.Queries;

public class SearchClientsHandler
{
    private readonly ZenTamDbContext _db;

    public SearchClientsHandler(ZenTamDbContext db)
    {
        _db = db;
    }

    public async Task<SearchClientsResponse> HandleAsync(SearchClientsRequest request, CancellationToken ct)
    {
        var query = _db.ClientProfiles.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            query = query.Where(c => c.PhoneNumber.Contains(request.Phone));
        }

        var clients = await query
            .Select(c => new ClientSummaryDto
            {
                Id = c.Id,
                Name = c.Name,
                PhoneNumber = c.PhoneNumber,
                SolarDob = c.SolarDob,
                Gender = c.Gender,
                Notes = c.Notes,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(ct);

        return new SearchClientsResponse { Clients = clients };
    }
}

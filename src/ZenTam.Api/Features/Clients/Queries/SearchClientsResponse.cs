using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients.Queries;

public class ClientSummaryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

public class SearchClientsResponse
{
    public List<ClientSummaryDto> Clients { get; init; } = new();
}

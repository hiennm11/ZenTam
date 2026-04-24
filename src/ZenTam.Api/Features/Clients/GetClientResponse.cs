using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients;

public class RelatedPersonDto
{
    public Guid Id { get; init; }
    public string Label { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
}

public class GetClientResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public List<RelatedPersonDto> RelatedPersons { get; init; } = new();
}

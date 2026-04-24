using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients;

public class AddRelatedPersonResponse
{
    public Guid Id { get; init; }
    public Guid ClientId { get; init; }
    public string Label { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
    public DateTime CreatedAt { get; init; }
}

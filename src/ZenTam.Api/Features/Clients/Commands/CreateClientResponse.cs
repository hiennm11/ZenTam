using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Features.Clients.Commands;

public class CreateClientResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public DateTime SolarDob { get; init; }
    public Gender Gender { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
}

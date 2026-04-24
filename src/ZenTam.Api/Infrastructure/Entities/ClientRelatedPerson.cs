using ZenTam.Api.Common.Domain;

namespace ZenTam.Api.Infrastructure.Entities;

public class ClientRelatedPerson
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string Label { get; set; } = string.Empty;
    public DateTime SolarDob { get; set; }
    public Gender Gender { get; set; }
    public DateTime CreatedAt { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;
}
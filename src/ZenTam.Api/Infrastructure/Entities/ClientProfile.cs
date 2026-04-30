using ZenTam.Api.Common.Domain;
using ZenTam.Api.Infrastructure.Entities;

namespace ZenTam.Api.Infrastructure.Entities;

public class ClientProfile
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime SolarDob { get; set; }
    public Gender Gender { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<ClientRelatedPerson> RelatedPersons { get; set; } = new List<ClientRelatedPerson>();
    public ICollection<ConsultationSession> ConsultationSessions { get; set; } = new List<ConsultationSession>();
}
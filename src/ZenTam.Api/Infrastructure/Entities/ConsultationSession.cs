namespace ZenTam.Api.Infrastructure.Entities;

public class ConsultationSession
{
    public Guid Id { get; set; }
    public Guid ClientId { get; set; }
    public string RawMessage { get; set; } = string.Empty;
    public string? ParsedIntents { get; set; }
    public string? EvaluationResult { get; set; }
    public DateTime CreatedAt { get; set; }

    public ClientProfile ClientProfile { get; set; } = null!;
}
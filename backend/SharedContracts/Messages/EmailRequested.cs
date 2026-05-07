namespace SharedContracts.Messages;

public record EmailRequested
{
    public Guid CorrelationId { get; set; }
    public string To { get; set; }
    public string Subject  { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = true;
    public Dictionary<string, string>? Attachments { get; set; } = null;
    public DateTime RequestedAt { get; set; } = default;
}
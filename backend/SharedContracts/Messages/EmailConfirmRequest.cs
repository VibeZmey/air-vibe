namespace SharedContracts.Messages;

public class EmailConfirmRequest
{
    public string Email { get; set; }
    public string Token { get; set; }
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
}
namespace SharedContracts.Messages;

public class OrderConfirmed
{
    public Guid OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public string Email { get; set; }
}
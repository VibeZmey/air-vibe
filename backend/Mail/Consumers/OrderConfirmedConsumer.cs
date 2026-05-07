using Mail.Services;
using MassTransit;
using SharedContracts.Messages;

namespace Mail.Consumers;

public class OrderConfirmedConsumer : IConsumer<OrderConfirmed>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<OrderConfirmedConsumer> _logger;
    private readonly IEmailTemplateService _templateService;
    
    public OrderConfirmedConsumer(
        IEmailService emailService,
        ILogger<OrderConfirmedConsumer> logger,
        IEmailTemplateService templateService)
    {
        _logger = logger;
        _emailService = emailService;
        _templateService = templateService;
    }
    
    public async Task Consume(ConsumeContext<OrderConfirmed> context)
    {
        var msg = context.Message;

        try
        {
            var (subject, body) = await _templateService
                .GetOrderConfirmedTemplateAsync(msg.OrderId, msg.TotalPrice, msg.Email);

            await _emailService.SendEmailAsync(msg.Email, subject, body);

            _logger.LogInformation("Order confirm email sent to {Email}", msg.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send order confirm email to {Email}", msg.Email);
            throw;
        }
    }
}
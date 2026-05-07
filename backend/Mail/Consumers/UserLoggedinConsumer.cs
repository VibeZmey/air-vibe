using Mail.Services;
using MassTransit;
using SharedContracts.Messages;

namespace Mail.Consumers;

public class UserLoggedinConsumer : IConsumer<UserLoggedin>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserLoggedinConsumer> _logger;
    private readonly IEmailTemplateService _templateService;

    public UserLoggedinConsumer(
        IEmailService emailService,
        ILogger<UserLoggedinConsumer> logger,
        IEmailTemplateService templateService)
    {
        _logger = logger;
        _emailService = emailService;
        _templateService = templateService;
    }

    public async Task Consume(ConsumeContext<UserLoggedin> context)
    {
        var msg = context.Message;

        try
        {
            var (subject, body) = await _templateService
                .GetLoginConfirmationTemplateAsync(msg.Login, msg.ConfirmationLink, msg.CreatedAt);

            await _emailService.SendEmailAsync(msg.Email, subject, body);

            _logger.LogInformation("Login email sent to {Email}", msg.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send login email to {Email}", msg.Email);
            throw;
        }
    }
}
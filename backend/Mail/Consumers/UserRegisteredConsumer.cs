using Mail.Services;
using MassTransit;
using Newtonsoft.Json;
using SharedContracts.Messages;

namespace Mail.Consumers;

public class UserRegisteredConsumer : IConsumer<UserRegistered>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<UserRegisteredConsumer> _logger;
    private readonly IEmailTemplateService _templateService;

    public UserRegisteredConsumer(
        IEmailService emailService,
        ILogger<UserRegisteredConsumer> logger,
        IEmailTemplateService templateService)
    {
        _emailService = emailService;
        _logger = logger;
        _templateService = templateService;
    }
    

    public async Task Consume(ConsumeContext<UserRegistered> context)
    {
        var msg = context.Message;

        try
        {
            var (subject, body) = await _templateService
                .GetRegistrationTemplateAsync(msg.Login, msg.ConfirmationLink, msg.CreatedAt);

            await _emailService.SendEmailAsync(msg.Email, subject, body);

            _logger.LogInformation("Registration email sent to {Email}", msg.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send registration email to {Email}", msg.Email);
            throw;
        }
    }
}
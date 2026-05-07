using GreenPipes;
using Mail.Consumers;
using Mail.Options;
using Mail.Services;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddOpenApi();

builder.Services.Configure<EmailSettings>(
    builder.Configuration.GetSection("EmailSettings"));

builder.Services.AddSingleton<IEmailService, EmailService>();
builder.Services.AddSingleton<IEmailTemplateService, EmailTemplateService>();

builder.Services.AddScoped<UserRegisteredConsumer>();
builder.Services.AddScoped<UserLoggedinConsumer>();
builder.Services.AddScoped<OrderConfirmedConsumer>();

var serviceProvider = builder.Services.BuildServiceProvider();
var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{ 
        var section = builder.Configuration.GetSection("RabbitMq");
        cfg.Host(
            section["HostName"] ?? "rabbitmq",
            section["VirtualHost"] ?? "/",
            h =>
            {
                h.Username(section["UserName"] ?? "guest");
                h.Password(section["Password"] ?? "guest");
            });
    
        cfg.ReceiveEndpoint("mail-service-queue", e =>
        {
            e.Consumer(() => serviceProvider.GetRequiredService<UserRegisteredConsumer>());
            e.Consumer(() => serviceProvider.GetRequiredService<UserLoggedinConsumer>());
            e.Consumer(() => serviceProvider.GetRequiredService<OrderConfirmedConsumer>());
            
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(5)));
        });
});
await busControl.StartAsync(new CancellationToken());

builder.Logging.AddFilter("MassTransit", LogLevel.Debug);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();

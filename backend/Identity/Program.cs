using System.Security.Cryptography;
using Identity;
using Identity.Data.Context;
using Identity.Options;
using Identity.Services.EmailTokenService;
using Identity.Services.JwtService;
using Identity.Services.UserService;
using MassTransit;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Flights API", 
        Version = "v1" 
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            [..Array.Empty<string>()]
        }
    });
});
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));
builder.Services.AddScoped<IIdentityDbContext>(provider => 
    provider.GetRequiredService<IdentityDbContext>());

builder.Services.AddControllers();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("JwtOptions"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddApiAuthentication(builder.Configuration, builder.Environment);
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
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
    });
});

builder.Logging.AddFilter("MassTransit", LogLevel.Debug);
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo("/app/Keys")) 
    .SetApplicationName("IdentityService");
builder.Services.AddScoped<IEmailTokenService, EmailTokenService>();

var app = builder.Build();

var rsaKey = app.Services.GetRequiredService<RSA>();
app.Lifetime.ApplicationStopping.Register(() => rsaKey.Dispose());

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<IdentityDbContext>();
        await context.Database.MigrateAsync();
        await context.SeedRolesAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred seeding the DB.");
    }
}
app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

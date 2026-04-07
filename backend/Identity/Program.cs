using System.Security.Cryptography;
using Identity;
using Identity.Data;
using Identity.Data.Context;
using Identity.Options;
using Identity.Services;
using Identity.Services.JwtService;
using Identity.Services.UserService;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<IdentityDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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


app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using Identity.Data.Context;
using Identity.Data.Models;
using Identity.Dto;
using Identity.Services.EmailTokenService;
using Identity.Services.JwtService;
using MassTransit;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SharedContracts.Messages;

namespace Identity.Services.UserService;

internal class UserService : IUserService
{
    private readonly IIdentityDbContext _context;
    private readonly IJwtService _jwtService;
    private readonly ILogger<UserService> _logger;
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IEmailTokenService _emailTokenService;
    
    public UserService(IIdentityDbContext context, 
        IJwtService jwtService,
        ILogger<UserService> logger,
        IPublishEndpoint publishEndpoint,
        IEmailTokenService emailTokenService)
    {
        _emailTokenService = emailTokenService;
        _jwtService = jwtService;
        _context = context;
        _logger = logger;
        _publishEndpoint = publishEndpoint;
    }
    
    public async Task Register(RegisterRequest user, CancellationToken ct = default)
    {
        var checkUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == user.Email, ct);
        
        if (checkUser is not null && checkUser.EmailConfirmed)
            throw new Exception("User is already exist");
        
        User newUser = new User()
        {
            Id = Guid.NewGuid(),
            Login = user.Login,
            Email = user.Email,
            CreatedAt = DateTime.UtcNow,
        };
        var passHash = new PasswordHasher<User>().HashPassword(newUser, user.Password);
        newUser.PasswordHash = passHash;
        
        var role = await _context
            .Roles
            .AsNoTracking()
            .FirstAsync(r => 
                r.Name == "User", ct);

        newUser.RoleId = role.Id;
        
        await _context.Users.AddAsync(newUser, ct);
        await _context.SaveChangesAsync(ct);
        
        var token = await _emailTokenService.GenerateToken(newUser.Id, user.Email, ct);
        //TODO: поменять ссылку на фронтовую
        var callbackUrl = $"http://localhost:8081/auth/confirm-email?token={Uri.EscapeDataString(token)}";
        
        await _publishEndpoint.Publish(new UserRegistered()
        {
            Email = user.Email,
            Login = user.Login,
            ConfirmationLink = callbackUrl,
            CreatedAt = DateTime.UtcNow,
        }, ct);
    }

    public async Task<JwtResponse?> ConfirmEmail(string token, CancellationToken ct = default)
    {
        var payload = await _emailTokenService.ValidateToken(token, ct);
    
        if (payload is null)
            throw new Exception("Invalid token");
        
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == payload.UserId, ct);
        
        if (user is null)
            throw new Exception("User is not found");

        user.EmailConfirmed = true;
        await _context.SaveChangesAsync(ct);
        return await _jwtService.GenerateJwt(user);
    }

    public async Task<bool> Login(LoginRequest loginUser, CancellationToken ct = default)
    {
        User? user = await _context
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u=> 
                u.Email == loginUser.Email, ct);

        if (user is null || user.IsBlocked)
            return false;
        
        var result = new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, loginUser.Password);

        if (result == PasswordVerificationResult.Failed)
            return false;
        
        var token = await _emailTokenService.GenerateToken(user.Id, user.Email, ct);
        //TODO: поменять ссылку на фронтовую
        var callbackUrl = $"http://localhost:8081/auth/confirm-email?token={Uri.EscapeDataString(token)}";

        await _publishEndpoint.Publish(new UserLoggedin()
        {
            Email = user.Email,
            Login = user.Login,
            ConfirmationLink = callbackUrl,
            CreatedAt = DateTime.UtcNow,
        }, ct);
        return true;
    }
    
    public async Task<GetUserResponse?> GetUserById(Guid id, CancellationToken ct = default)
    {
        var user = await _context
            .Users
            .FindAsync(id, ct);

        GetUserResponse response = new GetUserResponse()
        {
            Id = user.Id,
            RoleId = user.RoleId,
            Login = user.Login,
            Email = user.Email,
            Country = user.Country,
            Citizenship = user.Citizenship,
            Currency = user.Currency,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt,
            IsBlocked = user.IsBlocked,
        };
        
        return response;
    }

    public async Task Logout(Guid userId, CancellationToken ct = default)
    {
        await _jwtService.RevokeRefreshs(userId, ct);
        await _context.SaveChangesAsync(ct);
    }
    
}
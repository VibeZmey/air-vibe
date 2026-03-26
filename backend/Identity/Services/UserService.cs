using Identity.Data;
using Identity.Data.Models;
using Identity.Dto;
using Identity.Exceptions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class UserService
{
    private readonly IdentityDbContext _context;
    private readonly JwtService _jwtService;
    public UserService(IdentityDbContext context, JwtService jwtService)
    {
        _jwtService = jwtService;
        _context = context;
    }
    
    public async Task<JwtResponse?> Register(RegisterRequest user, CancellationToken ct = default)
    {
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
        
        return await _jwtService.GenerateJwt(newUser);
    }

    public async Task<JwtResponse?> Login(LoginRequest loginUser, CancellationToken ct = default)
    {
        User? user = await _context
            .Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u=> 
                u.Login == loginUser.Login, ct);

        if (user is null || user.IsBlocked)
            return null;
        
        var result = new PasswordHasher<User>()
            .VerifyHashedPassword(user, user.PasswordHash, loginUser.Password);

        if (result == PasswordVerificationResult.Failed)
            return null;
        
        return await _jwtService.GenerateJwt(user);
    }
    
    public async Task<GetUserResponse?> GetUserById(Guid id, CancellationToken ct = default)
    {
        var user = await _context
            .Users
            .FindAsync(id, ct);

        GetUserResponse response = new GetUserResponse()
        {
            Id = user.Id,
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
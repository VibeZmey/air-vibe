using Identity.Data.Context;
using Identity.Data.Models;
using Identity.Dto;
using Identity.Services.JwtService;
using Identity.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers;

[ApiController]
[AllowAnonymous]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtService _jwtService;
    private readonly IIdentityDbContext _context;
    
    
    public AuthController(
        IUserService userService, 
        IJwtService jwtService,
        IIdentityDbContext context)
    {
        _userService = userService;
        _jwtService = jwtService;
        _context = context;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterRequest request)
    {
        await _userService.Register(request);
        return Ok();
    }
    
    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginRequest request)
    {
        var res = await _userService.Login(request);
        return res ? Ok() : Unauthorized();
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<JwtResponse>> Refresh([FromBody] string token)
    {
        var res = await _jwtService.ValidateRefreshJwt(token);
        return res is null ? Unauthorized() : Ok(res);
    }

    [HttpPost("email-confirm")]
    public async Task<ActionResult<JwtResponse>> EmailConfirm([FromQuery] string token)
    {
        var res = await _userService.ConfirmEmail(token);
        return res is null ? Unauthorized() : Ok(res);
    }
    
    [HttpDelete("users")]
    public async Task<ActionResult<JwtResponse>> DeleteUsers()
    {
        _context.Users.RemoveRange(_context.Users);
        await _context.SaveChangesAsync();
        return Ok();
    }
}
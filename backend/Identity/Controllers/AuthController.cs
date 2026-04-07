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
    
    public AuthController(IUserService userService, IJwtService jwtService)
    {
        _userService = userService;
        _jwtService = jwtService;
    }
    
    [HttpPost("register")]
    public async Task<ActionResult<JwtResponse>> Register([FromBody] RegisterRequest request)
    {
        var res = await _userService.Register(request);
        return res is null ? Unauthorized() : Ok(res);
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<JwtResponse>> Login([FromBody] LoginRequest request)
    {
        var res = await _userService.Login(request);
        return res is null ? Unauthorized() : Ok(res);
    }
    
    [HttpPost("refresh")]
    public async Task<ActionResult<JwtResponse>> Refresh([FromBody] string token)
    {
        var res = await _jwtService.ValidateRefreshJwt(token);
        return res is null ? Unauthorized() : Ok(res);
    }
}
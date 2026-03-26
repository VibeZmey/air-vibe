using Identity.Dto;
using Identity.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Identity.Controllers;

[ApiController]
[Authorize]
[Route("users")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }
    
    [Authorize(Roles = "User, Admin, Supporter")]
    [HttpGet("me")]
    public async Task<ActionResult<GetUserResponse>> GetMe()
    {
        var userId = User.FindFirst("userId").Value;

        var user = await _userService.GetUserById(Guid.Parse(userId));
        return Ok(user);
    }
}
using Identity.Dto;

namespace Identity.Services.UserService;

public interface IUserService
{
    Task Logout(Guid userId, CancellationToken ct = default);
    Task<GetUserResponse?> GetUserById(Guid id, CancellationToken ct = default);
    Task<JwtResponse?> Login(LoginRequest loginUser, CancellationToken ct = default);
    Task<JwtResponse?> Register(RegisterRequest user, CancellationToken ct = default);
}
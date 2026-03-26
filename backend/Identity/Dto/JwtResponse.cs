namespace Identity.Dto;

public class JwtResponse
{
    public string Login { get; set; }
    public string? AccessToken { get; set; }
    public int ExpiresIn { get; set; }
    public string? RefreshToken { get; set; }
}
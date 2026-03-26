using System.ComponentModel.DataAnnotations;

namespace Identity.Dto;

public class LoginRequest
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }
}
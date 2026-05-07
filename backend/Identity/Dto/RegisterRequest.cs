using System.ComponentModel.DataAnnotations;

namespace Identity.Dto;

public class RegisterRequest
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }
    public string Email { get; set; }
}
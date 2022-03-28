using System.ComponentModel.DataAnnotations;

namespace UserManager.Contracts.DTO;

public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }

    [Required]
    public string Password { get; set; }
}

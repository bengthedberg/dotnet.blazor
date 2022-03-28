using System.ComponentModel.DataAnnotations;

namespace UserManager.Contracts.DTO;

public class RegisterRequest
{
    [Required]
    public string Password { get; set; }
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
    [Required]
    public string GivenName { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
}

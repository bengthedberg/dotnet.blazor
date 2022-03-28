using System.ComponentModel.DataAnnotations;

namespace UserManager.Contracts.DTO;

public class ForgotUserPasswordRequest
{
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}
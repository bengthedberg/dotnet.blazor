using System.ComponentModel.DataAnnotations;

namespace UserManager.Contracts.DTO;

public class ResetPasswordRequest
{
    [Required]
    public string UserId { get; set; }
    [Required]
    public string NewPassword { get; set; }
    [Required]
    public string ConfirmationCode { get; set; }
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
}
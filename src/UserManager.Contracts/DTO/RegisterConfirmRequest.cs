using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace UserManager.Contracts.DTO;

public class RegisterConfirmRequest
{
    [Required]
    public string ConfirmationCode { get; set; }    
    [Required]
    [EmailAddress]
    public string EmailAddress { get; set; }
    [Required]
    public string UserId { get; set; }
}

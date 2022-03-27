using System.ComponentModel.DataAnnotations;

namespace dotnet.blazor.client.Models;

public class LoginRequest
{

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100,MinimumLength = 6)]
        public string Password { get; set; } = string.Empty;

}

using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace UserManager.Contracts.DTO;

public class UpdateProfileRequest
{
    public string GivenName { get; set; }
    public string PhoneNumber { get; set; }
    public IFormFile ProfilePhoto { get; set; }
    public string Gender { get; set; }
    public string Address { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public string Pincode { get; set; }
    public string UserId { get; set; }
    [Required]
    public string AccessToken { get; set; }
}
using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;
public class RegisterResponse : BaseResponseModel
{
    public string EmailAddress { get; set; }
    public string UserId { get; set; }
}
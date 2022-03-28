using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;
public class LoginResponse : BaseResponseModel
{
    public string EmailAddress { get; set; }
    public string UserId { get; set; }
    public TokenModel Tokens { get; set; }  
}
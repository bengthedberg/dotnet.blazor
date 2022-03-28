using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;

public class ForgotUserPasswordResponse : BaseResponseModel
{
    public string UserId { get; set; }
    public string EmailAddress { get; set; }
}
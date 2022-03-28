using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;

public class LogoutResponse : BaseResponseModel
{
    public string UserId { get; set; }
}
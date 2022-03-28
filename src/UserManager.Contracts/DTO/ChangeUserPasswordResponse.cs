using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;
public class ChangeUserPasswordResponse : BaseResponseModel
{
    public string UserId { get; set; }
}

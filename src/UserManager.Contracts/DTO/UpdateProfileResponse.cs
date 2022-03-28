using UserManager.Contracts.Models;

namespace UserManager.Contracts.DTO;
public class UpdateProfileResponse : BaseResponseModel
{
    public string UserId { get; set; }
}
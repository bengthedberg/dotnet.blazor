using UserManager.Contracts.Enums;

namespace UserManager.Contracts.Models;
public class BaseResponseModel
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; }
    public StatusCodes Status { get; set; }
}
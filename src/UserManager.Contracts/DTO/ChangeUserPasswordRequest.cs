namespace UserManager.Contracts.DTO;

public class ChangeUserPasswordRequest
{
    public string CurrentPassword { get; set; }
    public string EmailAddress { get; set; }
    public string NewPassword { get; set; }
}
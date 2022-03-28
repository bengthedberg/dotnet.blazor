namespace UserManager.Contracts.DTO;

public class LogoutRequest
{
    public string UserId { get; set; }
    public string AccessToken { get; set; }
}

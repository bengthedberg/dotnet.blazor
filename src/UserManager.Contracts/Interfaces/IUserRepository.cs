using UserManager.Contracts.DTO;

namespace UserManager.Contracts.Interfaces;
public interface IUserRepository
{
    Task<RegisterResponse> ConfirmUserSignUpAsync(RegisterConfirmRequest request);
    Task<RegisterResponse> CreateUserAsync(RegisterRequest request);
    Task<UserProfileResponse> GetUserAsync(string userId);
    Task<ChangeUserPasswordResponse> TryChangePasswordAsync(ChangeUserPasswordRequest request);
    Task<ForgotUserPasswordResponse> TryInitForgotPasswordAsync(ForgotUserPasswordRequest request);
    Task<LoginResponse> TryLoginAsync(LoginRequest request);
    Task<LogoutResponse> TryLogOutAsync(LogoutRequest request);
    Task<ResetPasswordResponse> TryResetPasswordWithConfirmationCodeAsync(ResetPasswordRequest request);
    Task<UpdateProfileResponse> UpdateUserAttributesAsync(UpdateProfileRequest request);
}
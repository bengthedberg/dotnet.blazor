using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Extensions.CognitoAuthentication;
using UserManager.Contracts;
using UserManager.Contracts.DTO;
using UserManager.Contracts.Models;
using UserManager.Contracts.Enums;
using UserManager.Contracts.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using UserManager.Services.Models;
using StatusCodes = UserManager.Contracts.Enums.StatusCodes;

namespace UserManager.Services.Services;
public class UserRepository : IUserRepository
{
    private readonly AppConfig _cloudConfig;
    private readonly AmazonCognitoIdentityProviderClient _provider;
    private readonly CognitoUserPool _userPool;
    private readonly UserContextManager _userManager;
    private readonly HttpContext _httpContext;

    public UserRepository(IOptions<AppConfig> appConfig, UserContextManager userManager, IHttpContextAccessor httpContextAccessor)
    {
        _cloudConfig = appConfig.Value;
        _provider = new AmazonCognitoIdentityProviderClient(
            _cloudConfig.AccessKeyId, _cloudConfig.AccessSecretKey, RegionEndpoint.GetBySystemName(_cloudConfig.Region));
        _userPool = new CognitoUserPool(_cloudConfig.UserPoolId, _cloudConfig.AppClientId, _provider);
        _userManager = userManager;
        _httpContext = httpContextAccessor.HttpContext;
    }

    public async Task<RegisterResponse> CreateUserAsync(RegisterRequest request)
    {
        //// Register the user using Cognito
        var signUpRequest = new SignUpRequest
        {
            ClientId = _cloudConfig.AppClientId,
            Password = request.Password,
            Username = request.EmailAddress
        };

        signUpRequest.UserAttributes.Add(new AttributeType
        {
            Name = "email",
            Value = request.EmailAddress
        });
        signUpRequest.UserAttributes.Add(new AttributeType
        {
            Value = request.GivenName,
            Name = "given_name"
        });
        signUpRequest.UserAttributes.Add(new AttributeType
        {
            Value = request.PhoneNumber,
            Name = "phone_number"
        });

        //if (model.ProfilePhoto != null)
        //{
        //    // upload the incoming profile photo to user's S3 folder
        //    // and get the s3 url
        //    // add the s3 url to the profile_photo attribute of the userCognito
        //    var picUrl = await _storage.AddItem(model.ProfilePhoto, "profile");

        //    signUpRequest.UserAttributes.Add(new AttributeType
        //    {
        //        Value = picUrl,
        //        Name = "picture"
        //    });
        //}

        SignUpResponse response = await _provider.SignUpAsync(signUpRequest);

        var signUpResponse = new RegisterResponse
        {
            UserId = response.UserSub,
            EmailAddress = request.EmailAddress,
            Message = $"Confirmation Code sent to {response.CodeDeliveryDetails.Destination} via {response.CodeDeliveryDetails.DeliveryMedium.Value}",
            Status = StatusCodes.USER_UNCONFIRMED,
            IsSuccess = true
        };

        return signUpResponse;
    }

    public async Task<RegisterResponse> ConfirmUserSignUpAsync(RegisterConfirmRequest request)
    {
        ConfirmSignUpRequest confirmRequest = new ConfirmSignUpRequest
        {
            ClientId = _cloudConfig.AppClientId,
            ConfirmationCode = request.ConfirmationCode,
            Username = request.EmailAddress
        };
        var response = await _provider.ConfirmSignUpAsync(confirmRequest);

        // add to default users group
        //var addUserToGroupRequest = new AdminAddUserToGroupRequest
        //{
        //    UserPoolId = _cloudConfig.UserPoolId,
        //    Username = model.UserId,
        //    GroupName = "-users-group"
        //};
        //var addUserToGroupResponse = await _provider.AdminAddUserToGroupAsync(addUserToGroupRequest);

        return new RegisterResponse
        {
            EmailAddress = request.EmailAddress,
            UserId = request.UserId,
            Message = "User Confirmed",
            IsSuccess = true
        };
    }

    public async Task<ChangeUserPasswordResponse> TryChangePasswordAsync(ChangeUserPasswordRequest request)
    {
        // FetchTokens for User
        var tokenResponse = await AuthenticateUserAsync(request.EmailAddress, request.CurrentPassword);

        ChangePasswordRequest changePwdRequest = new ChangePasswordRequest
        {
            AccessToken = tokenResponse.Item2.AccessToken,
            PreviousPassword = request.CurrentPassword,
            ProposedPassword = request.NewPassword
        };
        ChangePasswordResponse response = await _provider.ChangePasswordAsync(changePwdRequest);
        return new ChangeUserPasswordResponse { UserId = tokenResponse.Item1.Username, Message = "Password Changed", IsSuccess = true };
    }

    public async Task<LoginResponse> TryLoginAsync(LoginRequest request)
    {
        try
        {
            var result = await AuthenticateUserAsync(request.EmailAddress, request.Password);

            if (result.Item1.Username != null)
            {
                await _userManager.SignIn(_httpContext, new Dictionary<string, string>() {
                    {ClaimTypes.Email, result.Item1.UserID},
                    {ClaimTypes.NameIdentifier, result.Item1.Username}
                });
            }

            var authResponseModel = new LoginResponse();
            authResponseModel.EmailAddress = result.Item1.UserID;
            authResponseModel.UserId = result.Item1.Username;
            authResponseModel.Tokens = new TokenModel
            {
                IdToken = result.Item2.IdToken,
                AccessToken = result.Item2.AccessToken,
                ExpiresIn = result.Item2.ExpiresIn,
                RefreshToken = result.Item2.RefreshToken
            };
            authResponseModel.IsSuccess = true;
            return authResponseModel;
        }
        catch (UserNotConfirmedException)
        {
            var listUsersResponse = await FindUsersByEmailAddress(request.EmailAddress);

            if (listUsersResponse != null && listUsersResponse.HttpStatusCode == HttpStatusCode.OK)
            {
                var users = listUsersResponse.Users;
                var filtered_user = users.FirstOrDefault(x => x.Attributes.Any(x => x.Name == "email" && x.Value == request.EmailAddress));

                var resendCodeResponse = await _provider.ResendConfirmationCodeAsync(new ResendConfirmationCodeRequest
                {
                    ClientId = _cloudConfig.AppClientId,
                    Username = filtered_user.Username
                });

                if (resendCodeResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = $"Confirmation Code sent to {resendCodeResponse.CodeDeliveryDetails.Destination} via {resendCodeResponse.CodeDeliveryDetails.DeliveryMedium.Value}",
                        Status = StatusCodes.USER_UNCONFIRMED,
                        UserId = filtered_user.Username
                    };
                }
                else
                {
                    return new LoginResponse
                    {
                        IsSuccess = false,
                        Message = $"Resend Confirmation Code Response: {resendCodeResponse.HttpStatusCode.ToString()}",
                        Status = StatusCodes.API_ERROR,
                        UserId = filtered_user.Username
                    };
                }
            }
            else
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = "No Users found for the EmailAddress.",
                    Status = StatusCodes.USER_NOTFOUND
                };
            }
        }
        catch (UserNotFoundException)
        {
            return new LoginResponse
            {
                IsSuccess = false,
                Message = "EmailAddress not found.",
                Status = StatusCodes.USER_NOTFOUND
            };
        }
        catch (NotAuthorizedException)
        {
            return new LoginResponse
            {
                IsSuccess = false,
                Message = "Incorrect username or password",
                Status = StatusCodes.API_ERROR
            };
        }
    }

    private async Task<Tuple<CognitoUser, AuthenticationResultType>> AuthenticateUserAsync(string emailAddress, string password)
    {
        CognitoUser user = new CognitoUser(emailAddress, _cloudConfig.AppClientId, _userPool, _provider);
        InitiateSrpAuthRequest authRequest = new InitiateSrpAuthRequest()
        {
            Password = password
        };

        AuthFlowResponse authResponse = await user.StartWithSrpAuthAsync(authRequest);
        var result = authResponse.AuthenticationResult;
        // return new Tuple<string, string, AuthenticationResultType>(user.UserID, user.Username, result);
        return new Tuple<CognitoUser, AuthenticationResultType>(user, result);
    }

    public async Task<LogoutResponse> TryLogOutAsync(LogoutRequest request)
    {
        var signoutRequest = new GlobalSignOutRequest { AccessToken = request.AccessToken };
        var response = await _provider.GlobalSignOutAsync(signoutRequest);

        await _userManager.SignOut(_httpContext);
        return new LogoutResponse { UserId = request.UserId, Message = "User Signed Out" };
    }

    public async Task<UpdateProfileResponse> UpdateUserAttributesAsync(UpdateProfileRequest request)
    {
        UpdateUserAttributesRequest userAttributesRequest = new UpdateUserAttributesRequest
        {
            AccessToken = request.AccessToken
        };

        userAttributesRequest.UserAttributes.Add(new AttributeType
        {
            Value = request.GivenName,
            Name = "given_name"
        });

        userAttributesRequest.UserAttributes.Add(new AttributeType
        {
            Value = request.PhoneNumber,
            Name = "phone_number"
        });

        // upload the incoming profile photo to user's S3 folder
        // and get the s3 url
        // add the s3 url to the profile_photo attribute of the userCognito
        // if (model.ProfilePhoto != null)
        // {
        //     var picUrl = await _storage.AddItem(model.ProfilePhoto, "profile");
        //     userAttributesRequest.UserAttributes.Add(new AttributeType
        //     {
        //         Value = picUrl,
        //         Name = "picture"
        //     });
        // }

        if (request.Gender != null)
        {
            userAttributesRequest.UserAttributes.Add(new AttributeType
            {
                Value = request.Gender,
                Name = "gender"
            });
        }

        if (!string.IsNullOrEmpty(request.Address) ||
            string.IsNullOrEmpty(request.State) ||
            string.IsNullOrEmpty(request.Country) ||
            string.IsNullOrEmpty(request.Pincode))
        {
            var dictionary = new Dictionary<string, string>();

            dictionary.Add("street_address", request.Address);
            dictionary.Add("region", request.State);
            dictionary.Add("country", request.Country);
            dictionary.Add("postal_code", request.Pincode);

            userAttributesRequest.UserAttributes.Add(new AttributeType
            {
                Value = JsonConvert.SerializeObject(dictionary),
                Name = "address"
            });
        }

        var response = await _provider.UpdateUserAttributesAsync(userAttributesRequest);
        return new UpdateProfileResponse { UserId = request.UserId, Message = "Profile Updated", IsSuccess = true };
    }

    public async Task<ForgotUserPasswordResponse> TryInitForgotPasswordAsync(ForgotUserPasswordRequest request)
    {
        var listUsersResponse = await FindUsersByEmailAddress(request.EmailAddress);

        if (listUsersResponse.HttpStatusCode == HttpStatusCode.OK)
        {
            var users = listUsersResponse.Users;
            var filtered_user = users.FirstOrDefault(x => x.Attributes.Any(x => x.Name == "email" && x.Value == request.EmailAddress));
            if (filtered_user != null)
            {
                var forgotPasswordResponse = await _provider.ForgotPasswordAsync(new ForgotPasswordRequest
                {
                    ClientId = _cloudConfig.AppClientId,
                    Username = filtered_user.Username
                });

                if (forgotPasswordResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    return new ForgotUserPasswordResponse
                    {
                        IsSuccess = true,
                        Message = $"Confirmation Code sent to {forgotPasswordResponse.CodeDeliveryDetails.Destination} via {forgotPasswordResponse.CodeDeliveryDetails.DeliveryMedium.Value}",
                        UserId = filtered_user.Username,
                        EmailAddress = request.EmailAddress,
                        Status = StatusCodes.USER_UNCONFIRMED
                    };
                }
                else
                {
                    return new ForgotUserPasswordResponse
                    {
                        IsSuccess = false,
                        Message = $"ListUsers Response: {forgotPasswordResponse.HttpStatusCode.ToString()}",
                        Status = StatusCodes.API_ERROR
                    };
                }
            }
            else
            {
                return new ForgotUserPasswordResponse
                {
                    IsSuccess = false,
                    Message = $"No users with the given emailAddress found.",
                    Status = StatusCodes.USER_NOTFOUND
                };
            }
        }
        else
        {
            return new ForgotUserPasswordResponse
            {
                IsSuccess = false,
                Message = $"ListUsers Response: {listUsersResponse.HttpStatusCode.ToString()}",
                Status = StatusCodes.API_ERROR
            };
        }
    }

    public async Task<ResetPasswordResponse> TryResetPasswordWithConfirmationCodeAsync(ResetPasswordRequest request)
    {
        var response = await _provider.ConfirmForgotPasswordAsync(new ConfirmForgotPasswordRequest
        {
            ClientId = _cloudConfig.AppClientId,
            Username = request.UserId,
            Password = request.NewPassword,
            ConfirmationCode = request.ConfirmationCode
        });

        if (response.HttpStatusCode == HttpStatusCode.OK)
        {
            return new ResetPasswordResponse
            {
                IsSuccess = true,
                Message = "Password Updated. Please Login."
            };
        }
        else
        {
            return new ResetPasswordResponse
            {
                IsSuccess = false,
                Message = $"ResetPassword Response: {response.HttpStatusCode.ToString()}",
                Status = StatusCodes.API_ERROR
            };
        }
    }

    private async Task<ListUsersResponse> FindUsersByEmailAddress(string emailAddress)
    {
        ListUsersRequest listUsersRequest = new ListUsersRequest
        {
            UserPoolId = _cloudConfig.UserPoolId,
            Filter = $"email=\"{emailAddress}\""
        };
        return await _provider.ListUsersAsync(listUsersRequest);
    }

    public async Task<UserProfileResponse> GetUserAsync(string userId)
    {
        var userResponse = await _provider.AdminGetUserAsync(new AdminGetUserRequest
        {
            Username = userId,
            UserPoolId = _cloudConfig.UserPoolId
        });

        // var user = _userPool.GetUser(userId);

        var attributes = userResponse.UserAttributes;
        var response = new UserProfileResponse
        {
            EmailAddress = attributes.GetValueOrDefault("email", string.Empty),
            GivenName = attributes.GetValueOrDefault("given_name", string.Empty),
            PhoneNumber = attributes.GetValueOrDefault("phone_number", string.Empty),
            Gender = attributes.GetValueOrDefault("gender", string.Empty),
            UserId = userId
        };

        var address = attributes.GetValueOrDefault("address", string.Empty);
        if (!string.IsNullOrEmpty(address))
        {
            response.Address = JsonConvert.DeserializeObject<Dictionary<string, string>>(address);
        }

        return response;
    }
}

internal static class AttributeTypeExtension
{
    public static string GetValueOrDefault(this List<AttributeType> attributeTypes, string propertyName, string defaultValue)
    {
        var prop = attributeTypes.FirstOrDefault(x => x.Name == propertyName);
        if (prop != null) return prop.Value;
        else return defaultValue;
    }
}


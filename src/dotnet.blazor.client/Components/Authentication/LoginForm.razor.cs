using Blazored.LocalStorage;
using dotnet.blazor.client.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http.Json;

namespace dotnet.blazor.client.Components;

public partial class LoginForm : ComponentBase
{

    [Inject]
    public HttpClient HttpClient { get; set; }

    [Inject]
    public NavigationManager Navigation { get; set; }

    [Inject]
    public AuthenticationStateProvider AuthenticationStateProvider { get; set; }

    [Inject]
    public ILocalStorageService Storage { get; set; }

    [Parameter]
    public string ReturnUrl { get; set; } = string.Empty;


    private LoginRequest _model = new LoginRequest();
    private bool _isBusy = false;
    private string _errorMessage = string.Empty;

    protected override void OnInitialized()
    {

    }

    private async Task LoginUserAsync()
    {
        _isBusy = true;
        _errorMessage = string.Empty;

        var response = await HttpClient.PostAsJsonAsync("/auth/login", _model); 
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            // Store it in local storage 
            await Storage.SetItemAsync("token", result.access_token);

            await AuthenticationStateProvider.GetAuthenticationStateAsync();

            System.Console.WriteLine($"url = {ReturnUrl}");
            if (string.IsNullOrEmpty(ReturnUrl))
                Navigation.NavigateTo("/");
            else
                Navigation.NavigateTo(ReturnUrl); 
        }
        else
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            _errorMessage = errorResult.Message; 
        }
        _isBusy = false; 
    }

    private void RedirectToRegister()
    {
        Navigation.NavigateTo("/authentication/register");
    }

}

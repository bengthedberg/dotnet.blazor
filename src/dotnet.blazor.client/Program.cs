using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using dotnet.blazor.client;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.UseLoadingBar();

builder.Services.AddHttpClient("Client.API", config =>
    {
        config.BaseAddress = new Uri("http://localhost:3000");
        config.Timeout = new TimeSpan(0, 0, 30);
        config.DefaultRequestHeaders.Clear();
    });

builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>().CreateClient("Client.API")
    .EnableIntercept(sp));

builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();
builder.Services.AddAuthorizationCore();

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLoadingBar(options =>
{
//  options.LoadingBarColor = "yellow";
}); 

builder.UseLoadingBar(); 

await builder.Build().RunAsync();
 
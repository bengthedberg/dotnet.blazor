using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using dotnet.blazor.client;
using MudBlazor.Services;
using Toolbelt.Blazor.Extensions.DependencyInjection;
using Blazored.LocalStorage;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.UseLoadingBar();

builder.Services.AddScoped(sp =>
    new HttpClient
    {
        BaseAddress = new Uri("http://localhost:3000")
    }.EnableIntercept(sp));

builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddLoadingBar(options =>
{
//  options.LoadingBarColor = "yellow";
}); 

builder.UseLoadingBar(); 

await builder.Build().RunAsync();
 
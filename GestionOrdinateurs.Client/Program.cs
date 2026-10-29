using Blazored.SessionStorage;
using GestionOrdinateurs.Client;
using GestionOrdinateurs.Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddBlazoredSessionStorage();

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<JwtAuthenticationStateProvider>());

builder.Services.AddTransient<JwtHandler>();

builder.Services.AddHttpClient("Gateway", client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:Gateway"]!))
    .AddHttpMessageHandler<JwtHandler>();

builder.Services.AddHttpClient("ApiOrdinateurs", client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:Ordinateurs"]!));

builder.Services.AddHttpClient("ApiAuth", client =>
    client.BaseAddress = new Uri(builder.Configuration["Api:Authentification"]!));

builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<OrdinateurApiService>();

await builder.Build().RunAsync();
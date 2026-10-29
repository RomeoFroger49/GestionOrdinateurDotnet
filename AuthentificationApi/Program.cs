using AuthentificationApi.Services;

var builder = WebApplication.CreateBuilder(args);

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5189"];

builder.Services.AddControllers();
builder.Services.AddHttpClient<KeycloakAuthService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (!builder.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");
app.MapControllers();

app.Run();
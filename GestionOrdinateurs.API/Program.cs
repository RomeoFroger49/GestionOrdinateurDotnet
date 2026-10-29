using System.Security.Claims;
using System.Text.Json;
using GestionOrdinateurs.API.Context;
using GestionOrdinateurs.API.Service;
using GestionOrdinateurs.Shared.Enums;
using GestionOrdinateurs.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection manquante");

var keycloakBaseUrl = builder.Configuration["Keycloak:BaseUrl"]
    ?? throw new InvalidOperationException("Keycloak:BaseUrl manquante");
var keycloakRealm = builder.Configuration["Keycloak:Realm"]
    ?? throw new InvalidOperationException("Keycloak:Realm manquante");
var keycloakIssuer = builder.Configuration["Keycloak:Issuer"]
    ?? $"{keycloakBaseUrl}/realms/{keycloakRealm}";
var keycloakClientId = builder.Configuration["Keycloak:ClientId"];

builder.Services.AddControllers();
builder.Services.AddDbContext<ParcInformatiqueContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddScoped<IOrdinateurService, OrdinateurService>();
builder.Services.AddScoped<IReparationService, ReparationService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<StockService>();
builder.Services.AddSingleton<JournalService>();
builder.Services.AddOpenApi();

var corsOrigins = builder.Configuration.GetSection("Cors:Origins").Get<string[]>()
    ?? ["http://localhost:5189"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorClient", policy =>
    {
        policy.WithOrigins(corsOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GestionOrdinateurs",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Collez uniquement le token (sans le mot Bearer)"
    });

    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("Bearer", document)] = []
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.MetadataAddress = $"{keycloakBaseUrl}/realms/{keycloakRealm}/.well-known/openid-configuration";
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.MapInboundClaims = false;
    options.TokenValidationParameters.ValidateIssuer = true;
    options.TokenValidationParameters.ValidIssuer = keycloakIssuer;
    options.TokenValidationParameters.ValidateAudience = true;
    options.TokenValidationParameters.ValidAudiences = new[] { "account", keycloakClientId }
        .Where(a => !string.IsNullOrEmpty(a))
        .Cast<string>();
    options.TokenValidationParameters.ValidateLifetime = true;
    options.TokenValidationParameters.ValidateIssuerSigningKey = true;
    options.TokenValidationParameters.NameClaimType = "preferred_username";
    options.TokenValidationParameters.RoleClaimType = "role";
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            if (context.Principal?.Identity is ClaimsIdentity identity)
            {
                AddKeycloakRoles(identity, keycloakClientId);
            }

            Console.WriteLine($"Utilisateur utilisant un Token Valide --- {context.Principal?.Identity?.Name ?? "Anonymous"}");
            return Task.CompletedTask;
        },
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"Token Invalide : {context.Exception.Message}");
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ParcInformatiqueContext>();

    db.Database.Migrate();

    DbInitializer.Initialize(db);

    if (!db.Users.Any())
    {
        db.Users.AddRange(
            new User { Login = "user1", Role = RoleEnum.Admin },
            new User { Login = "user2", Role = RoleEnum.Gestionnaire },
            new User { Login = "user3", Role = RoleEnum.Gestionnaire }
        );

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!builder.Configuration.GetValue<bool>("DOTNET_RUNNING_IN_CONTAINER"))
{
    app.UseHttpsRedirection();
}

app.UseCors("AllowBlazorClient");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static void AddKeycloakRoles(ClaimsIdentity identity, string? clientId)
{
    var roles = new List<string>();

    var realmAccess = identity.FindFirst("realm_access")?.Value;
    if (!string.IsNullOrEmpty(realmAccess))
    {
        using var realmDoc = JsonDocument.Parse(realmAccess);
        if (realmDoc.RootElement.TryGetProperty("roles", out var realmRoles))
        {
            roles.AddRange(realmRoles.EnumerateArray().Select(r => r.GetString() ?? string.Empty));
        }
    }

    var resourceAccess = identity.FindFirst("resource_access")?.Value;
    if (!string.IsNullOrEmpty(resourceAccess) && !string.IsNullOrEmpty(clientId))
    {
        using var resourceDoc = JsonDocument.Parse(resourceAccess);
        if (resourceDoc.RootElement.TryGetProperty(clientId, out var client)
            && client.TryGetProperty("roles", out var clientRoles))
        {
            roles.AddRange(clientRoles.EnumerateArray().Select(r => r.GetString() ?? string.Empty));
        }
    }

    foreach (var role in roles.Where(r => !string.IsNullOrEmpty(r)).Distinct())
    {
        if (!identity.HasClaim("role", role))
        {
            identity.AddClaim(new Claim("role", role));
        }
    }
}
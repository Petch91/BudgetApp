using System.Text;
using BudgetApp.Shared.Interfaces.Http;
using BudgetApp.Shared.Tools;
using Application.Interfaces;
using Application.Interfaces.Sécurité;
using Application.Persistence;
using Application.Services;
using Application.Tools.Sécurité;
using Front_BudgetApp.Api.Endpoints;
using Front_BudgetApp.Components;
using Front_BudgetApp.Services;
using Front_BudgetApp.Services.Notifications;
using Front_BudgetApp.Services.Sécurité;
using Front_BudgetApp.Services.Sécurité.Handlers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Serilog;
using AppToastService = Front_BudgetApp.Services.Notifications.AppToastService;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseStaticWebAssets();

SerilogConfiguration.Configure("ServerSide");

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();

/* =======================
 * SERVICES
 * ======================= */

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddBlazorBootstrap();
builder.Services.AddScoped<IAppToastService, AppToastService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "Budget App API", 
        Version = "v1",
        Description = "API de gestion de budget"
    });
    
    // Définir le schéma de sécurité JWT
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header en utilisant le schéma Bearer. Exemple: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    // Utiliser OpenApiSecuritySchemeReference pour la nouvelle API OpenAPI 2.x
    c.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", document),
            new List<string>()
        }
    });
});

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<DepenseFixeScheduler>();

builder.Services.AddScoped<IDepenseFixeService, DepenseFixeService>();
builder.Services.AddScoped<ITranscationService, TransactionService>();
builder.Services.AddScoped<ICategorieService, CategorieService>();
builder.Services.AddScoped<IRapportService, RapportService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Sécurité
builder.Services.AddScoped<IPasswordHasher, PasswordManager>();

builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection("Jwt"));

builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<JwtOptions>();

// Services d'authentification Blazor
builder.Services.AddScoped<ProtectedLocalStorage>();
builder.Services.AddScoped<AuthStateService>();
builder.Services.AddScoped<AuthenticationStateProvider,CustomAuthStateProvider>();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<JwtAuthorizationHandler>();


builder.Services.AddHttpClient(
    "Api", x => x.BaseAddress = new Uri("http://localhost:5201/api/"));
    //.AddHttpMessageHandler<JwtAuthorizationHandler>();

builder.Services.AddScoped<IHttpCategorie, CategorieFrontService>();
builder.Services.AddScoped<IHttpDepenseFixe, DepenseFixeFrontService>();
builder.Services.AddScoped<IHttpTransaction, TransactionFrontService>();
builder.Services.AddScoped<IHttpRapport, RapportFrontService>();

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Connected", policy =>
        policy.RequireAuthenticatedUser());

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtOptions = builder.Configuration
            .GetSection("Jwt")
            .Get<JwtOptions>() ?? new JwtOptions();

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.Secret))
        };
    });

var app = builder.Build();

/* =======================
 * PIPELINE HTTP
 * ======================= */

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

/* 🔥 OBLIGATOIRE AVANT TOUT */
app.UseStaticFiles();
app.UseAntiforgery();
// Auth
app.UseAuthentication();
app.UseAuthorization();

/* API */
app.MapDepenseFixe();
app.MapTransactionVariable();
app.MapCategorie();
app.MapRapport();
app.MapAuth();

/* BLAZOR */
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode().AllowAnonymous();

app.Run();
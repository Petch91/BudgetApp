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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using AppToastService = Front_BudgetApp.Services.Notifications.AppToastService;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<MyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHostedService<DepenseFixeScheduler>();

builder.Services.AddScoped<IDepenseFixeService, DepenseFixeService>();
builder.Services.AddScoped<ITranscationService, TransactionService>();
builder.Services.AddScoped<ICategorieService, CategorieService>();
builder.Services.AddScoped<IRapportService, RapportService>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<IPasswordHasher, PasswordManager>();
builder.Services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddSingleton<JwtOptions>();


builder.Services.AddHttpClient(
    "Api", x => x.BaseAddress = new Uri("http://localhost:5201/api/"));

builder.Services.AddScoped<IHttpCategorie, CategorieFrontService>();
builder.Services.AddScoped<IHttpDepenseFixe, DepenseFixeFrontService>();
builder.Services.AddScoped<IHttpTransaction, TransactionFrontService>();
builder.Services.AddScoped<IHttpRapport, RapportFrontService>();

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

builder.Services.AddAuthorization();

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
    app.UseHsts();
}

app.UseHttpsRedirection();

/* 🔥 OBLIGATOIRE AVANT TOUT */
app.UseStaticFiles();
app.UseAntiforgery();

/* API */
app.MapDepenseFixe();
app.MapTransactionVariable();
app.MapCategorie();
app.MapRapport();

/* BLAZOR */
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
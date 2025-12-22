using BudgetApp.Shared.Interfaces.Http;
using BudgetApp.Shared.Tools;
using Datas.Interfaces;
using Datas.Persistence;
using Datas.Services;
using Front_BudgetApp.Api.Endpoints;
using Front_BudgetApp.Components;
using Front_BudgetApp.Services;
using Front_BudgetApp.Services.Notifications;
using Microsoft.EntityFrameworkCore;
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

builder.Services.AddScoped<IDepenseFixeService, DepenseFixeService>();
builder.Services.AddScoped<ITranscationService, TransactionService>();
builder.Services.AddScoped<ICategorieService, CategorieService>();

builder.Services.AddHttpClient(
    "Api", x => x.BaseAddress = new Uri("http://localhost:5201/api/"));

builder.Services.AddScoped<IHttpCategorie, CategorieFrontService>();
builder.Services.AddScoped<IHttpDepenseFixe, DepenseFixeFrontService>();

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

/* BLAZOR */
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
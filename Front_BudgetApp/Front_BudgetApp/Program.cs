using API_BudgetApp.Endpoints;
using BudgetApp.Shared.Interfaces.Http;
using BudgetApp.Shared.Tools;
using Datas;
using Datas.Services;
using Datas.Services.Interfaces;
using Front_BudgetApp.Client.Pages;
using Front_BudgetApp.Components;
using Front_BudgetApp.Services;
using Microsoft.EntityFrameworkCore;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

SerilogConfiguration.Configure("ServerSide");

builder.Logging.ClearProviders();
builder.Logging.AddSerilog();


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddBlazorBootstrap();

builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options => { options.DetailedErrors = true; });

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapDepenseFixe();
app.MapTransactionVariable();
app.MapCategorie();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Front_BudgetApp.Client._Imports).Assembly);

app.Run();
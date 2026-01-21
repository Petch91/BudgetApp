using Application.Interfaces;
using Application.Interfaces.Sécurité;
using Entities.Contracts.Forms;
using Microsoft.AspNetCore.Http;

namespace Front_BudgetApp.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth")
            .WithTags("Authentification");

        /* =======================
         * Login
         * ======================= */
        group.MapPost("/login", async (
            LoginForm form,
            IAuthService service) =>
        {
            var result = await service.LoginAsync(form);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * Register
         * ======================= */
        group.MapPost("/register", async (
            RegisterForm form,
            IAuthService service) =>
        {
            var result = await service.RegisterAsync(form);

            return result.IsSuccess
                ? Results.Created("/api/auth/me", result.Value)
                : Results.BadRequest(result.Errors);
        });

        /* =======================
         * Refresh Token
         * ======================= */
        group.MapPost("/refresh", async (
            RefreshTokenForm form,
            IAuthService service) =>
        {
            var result = await service.RefreshToken(form);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : Results.Unauthorized();
        });

        /* =======================
         * Logout
         * ======================= */
        group.MapPost("/logout", async (
            RefreshTokenForm form,
            IAuthService service) =>
        {
            var result = await service.Logout(form.RefreshToken);

            return result.IsSuccess
                ? Results.Ok()
                : Results.BadRequest(result.Errors);
        });
    }
}

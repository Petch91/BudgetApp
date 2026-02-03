using System.Net;

namespace Front_BudgetApp.Services;

public static class HttpErrorHelper
{
    public static string GetUserMessage(HttpResponseMessage response, string operation)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "Session expirée, veuillez vous reconnecter",
            HttpStatusCode.Forbidden => "Vous n'avez pas les droits pour cette action",
            HttpStatusCode.NotFound => $"{operation} : élément non trouvé",
            HttpStatusCode.BadRequest => $"{operation} : données invalides",
            HttpStatusCode.InternalServerError => "Erreur serveur, réessayez plus tard",
            _ => $"Erreur lors de : {operation}"
        };
    }
}

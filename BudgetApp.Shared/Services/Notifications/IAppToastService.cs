namespace Front_BudgetApp.Services.Notifications;

public interface IAppToastService
{
    void Success(string message, string title = null);
    void Info(string message, string title = null);
    void Warning(string message, string title = null);
    void Error(string message, string title = null);
    void ExecuteQueue();
}
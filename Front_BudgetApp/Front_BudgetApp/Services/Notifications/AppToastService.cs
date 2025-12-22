using BlazorBootstrap;

namespace Front_BudgetApp.Services.Notifications;

public class AppToastService(ToastService toastService) : IAppToastService
{
    private readonly Queue<ToastMessage> _queue = new();
    private bool _uiReady;

    public void Success(string message, string? title = null)
        => Enqueue(ToastType.Success, message, title, autoHide: true);

    public void Info(string message, string? title = null)
        => Enqueue(ToastType.Info, message, title, autoHide: true);

    public void Warning(string message, string? title = null)
        => Enqueue(ToastType.Warning, message, title, autoHide: false);

    public void Error(string message, string? title = null)
        => Enqueue(ToastType.Danger, message, title, autoHide: false);

    private void Enqueue(ToastType type, string message, string? title, bool autoHide)
    {
        _queue.Enqueue(new ToastMessage
        {
            Type = type,
            Message = message,
            Title = title,
            AutoHide = autoHide
        });

        TryFlush();
    }

    public void ExecuteQueue()
    {
        _uiReady = true;
        TryFlush();
    }

    private void TryFlush()
    {
        if (!_uiReady)
            return;

        while (_queue.Count > 0)
        {
            try
            {
                toastService.Notify(_queue.Dequeue());
            }
            catch
            {
                // ⚠️ Un toast ne doit JAMAIS casser l’app
                break;
            }
        }
    }
}
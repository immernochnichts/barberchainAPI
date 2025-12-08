using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public interface INotificationService
    {
        Task NotifyAsync(int accId, string content, NotificationType notType);
        Task NotifyAsync(int accId, Notification not);
    }
}

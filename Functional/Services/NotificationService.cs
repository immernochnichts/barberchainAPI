using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public class NotificationService : INotificationService
    {
        private BarberchainDbContext _context;

        public NotificationService(BarberchainDbContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Creates a new notification in the database with specified content and type
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="content"></param>
        /// <param name="notType"></param>
        /// <returns></returns>
        public async Task NotifyAsync(int accId, string content, NotificationType notType = NotificationType.General)
        {
            Notification not = new Notification()
            {
                Content = content,
                Type = notType
            };

            _context.Notifications.Add(not);
            await _context.SaveChangesAsync();

            AccountNotification acc_not = new AccountNotification()
            {
                AccountId = accId,
                NotificationId = not.Id
            };

            _context.AccountNotifications.Add(acc_not);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        ///     Uses provided notification to link it with account in many-to-many table. Allows to avoid notification duplicates
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="not"></param>
        /// <returns></returns>
        public async Task NotifyAsync(int accId, Notification not)
        {
            _context.Notifications.Add(not);
            await _context.SaveChangesAsync();

            AccountNotification acc_not = new AccountNotification()
            {
                AccountId = accId,
                NotificationId = not.Id
            };

            _context.AccountNotifications.Add(acc_not);
            await _context.SaveChangesAsync();
        }
    }
}

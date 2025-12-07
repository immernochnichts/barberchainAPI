using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public class OrderService : IOrderService
    {
        private BarberchainDbContext _context;

        public OrderService(BarberchainDbContext context)
        {
            _context = context;
        }

        ///<summary>
        ///  Order status has to be set outside of the method
        ///</summary>
        public async Task AddOrderToScheduleAsync(BarberScheduleDay bsd, Order order)
        {
            int startIndex =
               order.AppointedTime.Hour * 4 +
               order.AppointedTime.Minute / 15;

            int totalDuration = order.OrderJobs.Sum(j => (int)j.Job.DurationAtu);

            for (int i = startIndex; i < startIndex + totalDuration; i++)
            {
                if (i >= 0 && i < 96)
                {
                    bsd.AtuPattern[i] = false;
                }
            }

            _context.BarberScheduleDays.Update(bsd);
            await _context.SaveChangesAsync();
        }

        ///<summary>
        ///  Order status has to be set outside of the method
        ///</summary>
        public async Task EraseOrderFromScheduleAsync(BarberScheduleDay bsd, Order order, bool notifyBarber = false)
        {
            int startIndex =
               order.AppointedTime.Hour * 4 +
               order.AppointedTime.Minute / 15;

            int totalDuration = order.OrderJobs.Sum(j => (int)j.Job.DurationAtu);

            for (int i = startIndex; i < startIndex + totalDuration; i++)
            {
                if (i >= 0 && i < 96)
                {
                    bsd.AtuPattern[i] = true;
                }
            }
            _context.BarberScheduleDays.Update(bsd);

            if (notifyBarber && bsd.Barber.AccountId != null)
            {
                Notification not = new Notification()
                {
                    Content = $"Заказ #{order.Id} был отменён",
                    Type = NotificationType.General
                };

                _context.Notifications.Add(not);
                await _context.SaveChangesAsync();

                AccountNotification acc_not = new AccountNotification()
                {
                    AccountId = bsd.Barber.AccountId.Value,
                    NotificationId = not.Id
                };

                _context.AccountNotifications.Add(acc_not);
            }

            await _context.SaveChangesAsync();
        }
    }
}

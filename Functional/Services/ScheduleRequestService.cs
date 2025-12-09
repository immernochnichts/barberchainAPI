using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Collections;
using System.Text;

namespace barberchainAPI.Functional.Services
{
    public class ScheduleRequestService : IScheduleRequestService
    {
        private readonly BarberchainDbContext _context;
        private readonly IOrderService _orderService;
        private readonly INotificationService _notificationService;

        public ScheduleRequestService(BarberchainDbContext context, IOrderService orderService, INotificationService notificationService)
        {
            _context = context;
            _orderService = orderService;
            _notificationService = notificationService;
        }

        public async Task<List<ScheduleRequest>> LoadRequestsAsync(int managerId)
        {
            return await _context.ScheduleRequests
                .Include(r => r.Barber)
                    .ThenInclude(b => b.Account)
                .Include(r => r.Barber)
                    .ThenInclude(b => b.Bshop)
                .Where(r => r.Barber.Bshop.ManagerAccountId == managerId)
                .OrderByDescending(r => r.RequestDate)
                .ToListAsync();
        }

        public async Task ApproveRequestAsync(ScheduleRequest req)
        {
            var declinedOrderCount = !string.IsNullOrEmpty(req.OrderIdsToDecline) ? req.OrderIdsToDecline?.Split(" ").Count() : 0;

            if (req != null)
            {
                var sched = await _context.BarberScheduleDays.Where(d => d.Date == req.RequestDate && d.BarberId == req.BarberId).FirstOrDefaultAsync();
                req.Status = ScheduleRequestStatus.Approved;

                if (sched == null) // barber altered schedule that's not present in DB yet. We'll create it right now then. No orders could've been placed here
                {
                    var newSched = new BarberScheduleDay()
                    {
                        AtuPattern = req.AtuPattern,
                        BarberId = req.BarberId,
                        Date = req.RequestDate
                    };
                    _context.BarberScheduleDays.Add(newSched);
                }
                else // barber altered schedule that's present in the DB
                {
                    sched.AtuPattern = req.AtuPattern;

                    await DeclineOrdersAsync(req);
                }

                await _context.SaveChangesAsync();
                await NotifyBarberAsync(req);
            }
        }

        private async Task DeclineOrdersAsync(ScheduleRequest req)
        {
            bool firstTime = true;
            Notification not = default!;

            foreach (var o in req!.OrderIdsToDecline!.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            {
                var order = await _context.Orders
                    .Where(ord => ord.Id == int.Parse(o))
                    .Include(o => o.OrderJobs)
                        .ThenInclude(oj => oj.Job)
                    .FirstOrDefaultAsync();

                if (order!.AccountId != null)
                {
                    if (firstTime)
                    {
                        not = new Notification // send notification to users with an account
                        {
                            Type = NotificationType.General,
                            Content = $"Просим прощения за предоставленные неудобства. Ваш заказ был отклонён. Сотрудник позвонит на номер телефона вашего профиля."
                        };
                        _context.Notifications.Add(not);
                        await _context.SaveChangesAsync();
                        firstTime = false;
                    }

                    var acc_not = new AccountNotification
                    {
                        AccountId = order.AccountId.Value,
                        NotificationId = not.Id
                    };

                    var existsLocal = _context.AccountNotifications.Local
                        .Any(an => an.AccountId == acc_not.AccountId && an.NotificationId == acc_not.NotificationId);

                    if (!existsLocal)
                    {
                        var existsDb = await _context.AccountNotifications
                            .AnyAsync(an => an.AccountId == acc_not.AccountId && an.NotificationId == acc_not.NotificationId);

                        if (!existsDb)
                            _context.AccountNotifications.Add(acc_not);
                    }
                }

                order.Status = OrderStatus.Declined;

                //var bsd = await _context.BarberScheduleDays.Where(d => d.Date == req.RequestDate && req.BarberId == d.BarberId).FirstAsync();

                //await _orderService.EraseOrderFromScheduleAsync(bsd, order);
            }
        }

        public async Task NotifyBarberAsync(ScheduleRequest req)
        {
            Notification not = default!;

            if (req.Status == ScheduleRequestStatus.Approved)
            {
                List<Order> orders = new();

                var orderIdsToDeclineArray = req.OrderIdsToDecline?.Split(" ");
                foreach (var o in req.OrderIdsToDecline!.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                {
                    int orderId = int.Parse(o);

                    var order = await _context.Orders
                        .Include(o => o.Account)
                        .Where(ord => ord.Id == orderId)
                        .FirstOrDefaultAsync();

                    orders.Add(order!);
                }

                not = new Notification
                {
                    Type = NotificationType.General,
                    Content = $"Запрос на изменение расписания на {req.RequestDate} одобрен. Заказов отклонено: {req.OrderIdsToDecline.Split(" ", StringSplitOptions.RemoveEmptyEntries).Count()}. Пожалуйста, позвоните на следующие номера телефонов и предупредите об отмене заказов: {string.Join(", ", orders.Select(o => o.AccountId != null ? o.Account.Phone : o.Phone))}"
                };
            }
            else
            {
                not = new Notification
                {
                    Type = NotificationType.General,
                    Content = $"Запрос на изменение расписания на {req.RequestDate} отклонён. Менеджер указал следующую причину: {req.ReasonRejected}" // the reason is null or empty or whatever the fuck
                };
            }

            await _notificationService.NotifyAsync(req.Barber.AccountId!.Value, not);
        }
    }
}

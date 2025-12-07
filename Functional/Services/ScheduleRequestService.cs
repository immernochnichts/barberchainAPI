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

        public ScheduleRequestService(BarberchainDbContext context)
        {
            _context = context;
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
                var order = await _context.Orders.Where(ord => ord.Id == int.Parse(o)).FirstOrDefaultAsync();

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

                //clear declined orders from schedule
                int startIndex =
                   order.AppointedTime.Hour * 4 +
                   order.AppointedTime.Minute / 15;

                int totalDuration = order.OrderJobs.Sum(j => (int)j.Job.DurationAtu);

                for (int i = startIndex; i < totalDuration; i++)
                {
                    if (i >= 0 && i < 96)
                    {
                        req.AtuPattern[i] = true;
                    }
                }
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

            _context.Notifications.Add(not);
            await _context.SaveChangesAsync();

            var acc_not = new AccountNotification
            {
                AccountId = req.Barber.AccountId!.Value,
                NotificationId = not.Id
            };

            _context.AccountNotifications.Add(acc_not);
            await _context.SaveChangesAsync();
        }

        public string GetScheduleChangesAsync(int requestId)
        {
            var req = _context.ScheduleRequests.Where(r => r.Id == requestId).FirstOrDefault();
            var sched = _context.BarberScheduleDays.Where(d => d.Date == req!.RequestDate && d.BarberId == req.BarberId).FirstOrDefault();

            return GetAtuDiff(sched == null ? req!.Barber.Bshop.DefaultSchedule : sched.AtuPattern, req!.AtuPattern);
        }

        public static string GetAtuDiff(BitArray before, BitArray after)
        {
            var availableRanges = new List<string>();
            var unavailableRanges = new List<string>();

            for (int i = 0; i < 96; i++)
            {
                bool b = before[i];
                bool a = after[i];

                if (b == a)
                    continue;

                bool available = !b && a;
                bool unavailable = b && !a;

                int start = i;

                while (i + 1 < 96 && before[i + 1] != after[i + 1])
                    i++;

                int end = i;

                string from = AtuIndexToTime(start);
                string to = AtuIndexToTime(end + 1);

                string range = $"{from}–{to}";

                if (available)
                    availableRanges.Add(range);
                else
                    unavailableRanges.Add(range);
            }

            var sb = new StringBuilder();

            if (availableRanges.Count > 0)
                sb.AppendLine($"{string.Join(", ", availableRanges)} стало доступно");

            if (unavailableRanges.Count > 0)
                sb.AppendLine($"{string.Join(", ", unavailableRanges)} стало недоступно");

            return sb.ToString().TrimEnd();
        }

        private static string AtuIndexToTime(int index)
        {
            int minutes = index * 15;
            return TimeSpan.FromMinutes(minutes).ToString(@"hh\:mm");
        }
    }
}

using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Security.Claims;
using System.Text;

namespace barberchainAPI.Functional.Services
{
    public class BarberScheduleLoadResult
    {
        public BitArray AtuPattern { get; set; } = default!;
        public List<Order> Orders { get; set; } = default!;
        public List<int> OccupiedIndexes { get; set; } = default!;
        public Dictionary<Tuple<int, int>, Order> IndexRangeToOrder { get; set; } = default!;
    }

    public class CreateReplaceSchedDto
    {
        public Barber Barber { get; set; } = default!;
        public DateOnly SelectedDate { get; set; }
        public BitArray AtuPattern { get; set; } = default!;
        public string Message { get; set; } = default!;
        public List<Order> OrdersToDecline { get; set; } = new();
    }

    public class CreateReplaceSchedReqResult
    {
        public string Error { get; set; } = default!;
        public ScheduleRequest Request { get; set; } = default!;
    }

    public class CancelChangesDto
    {
        public DateOnly SelectedDate { get; set; } = default!;
        public Barber Barber { get; set; } = default!;
        public BitArray AtuPattern { get; set; } = default!;
        public List<Order> OrdersToDecline { get; set; } = default!;
        public List<Order> Orders { get; set; } = default!;
        public List<int> OccupiedIndexes { get; set; } = default!;
    }

    public class CreateScheduleService : ICreateScheduleService
    {
        private readonly BarberchainDbContext _context;

        public CreateScheduleService(BarberchainDbContext context)
        {
            _context = context;
        }

        public async Task<BarberScheduleLoadResult> LoadBarberScheduleAsync(int barberId, DateOnly date)
        {
            var barber = (await _context.Barbers.FindAsync(barberId))!;

            var bsd = await _context.BarberScheduleDays
                .Where(x => x.Date == date && x.BarberId == barber.Id)
                .FirstOrDefaultAsync();

            var atuPattern = bsd != null ? new BitArray(bsd.AtuPattern) : new BitArray(barber.Bshop.DefaultSchedule);

            var orders = await _context.Orders
                .Where(o =>
                    DateOnly.FromDateTime(o.AppointedTime) == date &&
                    o.BarberId == barber.Id &&
                    (o.Status == OrderStatus.Pending || o.Status == OrderStatus.Waiting || o.Status == OrderStatus.Processing))
                .Include(o => o.OrderJobs)
                .ToListAsync();

            var occupiedIndexes = new List<int>();
            Dictionary<Tuple<int, int>, Order> idxRangeToOrder = new();

            foreach (var order in orders)
            {
                int startIndex =
                    order.AppointedTime.Hour * 4 +
                    order.AppointedTime.Minute / 15;

                int totalDuration = order.OrderJobs.Sum(j => (int)j.Job.DurationAtu);

                idxRangeToOrder.Add(new Tuple<int, int>(startIndex, startIndex + totalDuration - 1), order);

                for (int i = 0; i < totalDuration; i++)
                {
                    int idx = startIndex + i;
                    if (idx >= 0 && idx < 96)
                        occupiedIndexes.Add(idx);
                }
            }

            return new BarberScheduleLoadResult()
            {
                AtuPattern = atuPattern,
                IndexRangeToOrder = idxRangeToOrder,
                OccupiedIndexes = occupiedIndexes,
                Orders = orders
            };
        }

        private bool BitArraysEqual(BitArray a, BitArray b)
        {
            if (a == null || b == null) return false;
            if (a.Length != b.Length) return false;

            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i])
                    return false;

            return true;
        }

        public async Task NotifyManagerAsync(Barber barber, DateTime selectedDate)
        {
            var not = new Notification
            {
                Type = NotificationType.System,
                Content = $"Запрос на изменение расписания от {barber.Account.Lastname} {barber.Account.Restname} за {selectedDate.ToString("yyyy-MM-dd")}"
            };

            _context.Notifications.Add(not);
            await _context.SaveChangesAsync();

            var acc_not = new AccountNotification
            {
                AccountId = barber.Bshop.ManagerAccountId!.Value,
                NotificationId = not.Id
            };

            _context.AccountNotifications.Add(acc_not);
            await _context.SaveChangesAsync();
        }

        public async Task<CreateReplaceSchedReqResult> CreateOrReplaceScheduleRequest(CreateReplaceSchedDto dto)
        {
            var bsd = await _context.BarberScheduleDays
                .Where(d => d.Date == dto.SelectedDate && d.BarberId == dto.Barber.Id)
                .Select(d => d.AtuPattern)
                .FirstOrDefaultAsync();

            if ((bsd != null && BitArraysEqual(bsd, dto.AtuPattern)) && dto.OrdersToDecline.Count == 0 || BitArraysEqual(dto.AtuPattern, dto.Barber.Bshop.DefaultSchedule))
            {
                return new CreateReplaceSchedReqResult()
                {
                    Error = "Вы не внесли никаких изменений",
                    Request = null!
                };
            }

            //var barberId = await _context.Barbers.Where(b => b.AccountId == dto.UserId).Select(b => b.Id).FirstOrDefaultAsync(); // not sure if userId is needed here
            var requestDate = dto.SelectedDate;

            var alreadyExistingRequest = await _context.ScheduleRequests.Where(sr => sr.RequestDate == requestDate && sr.BarberId == dto.Barber.Id).FirstOrDefaultAsync();

            if (alreadyExistingRequest != null)
            {
                alreadyExistingRequest.AtuPattern = dto.AtuPattern;
                alreadyExistingRequest.Message = dto.Message;
                alreadyExistingRequest.Status = ScheduleRequestStatus.Pending;
                alreadyExistingRequest.OrderIdsToDecline = string.Join(" ", dto.OrdersToDecline.Select(o => o.Id.ToString()));
                alreadyExistingRequest.Changes = GetScheduleChanges(alreadyExistingRequest.Id);
                await _context.SaveChangesAsync();
                return new CreateReplaceSchedReqResult()
                {
                    Request = alreadyExistingRequest,
                    Error = null!
                };
            }
            else
            {
                CreateReplaceSchedReqResult result = new CreateReplaceSchedReqResult()
                {
                    Request = new ScheduleRequest
                    {
                        BarberId = dto.Barber.Id,
                        RequestDate = requestDate,
                        AtuPattern = dto.AtuPattern,
                        Message = dto.Message,
                        Status = ScheduleRequestStatus.Pending,
                        OrderIdsToDecline = string.Join(" ", dto.OrdersToDecline.Select(o => o.Id.ToString())),
                    },
                    Error = null!
                };

                _context.ScheduleRequests.Add(result.Request);
                await _context.SaveChangesAsync();

                result.Request.Changes = GetScheduleChanges(result.Request.Id);

                await _context.SaveChangesAsync();
                return result;
            }
        }

        public async Task CancelChangesAsync(CancelChangesDto dto)
        {
            var bsd = await _context.BarberScheduleDays
                .Where(x => x.Date == dto.SelectedDate && x.BarberId == dto.Barber.Id)
                .FirstOrDefaultAsync();

            dto.AtuPattern.SetAll(true);
            var sched = bsd != null ? bsd.AtuPattern : dto.Barber.Bshop.DefaultSchedule;
            dto.AtuPattern.And(sched);
            dto.OrdersToDecline.Clear();

            foreach (var order in dto.Orders)
            {
                int startIndex =
                    order.AppointedTime.Hour * 4 +
                    order.AppointedTime.Minute / 15;

                int totalDuration = order.OrderJobs.Sum(j => (int)j.Job.DurationAtu);

                for (int i = 0; i < totalDuration; i++)
                {
                    int idx = startIndex + i;
                    if (idx >= 0 && idx < 96)
                        dto.OccupiedIndexes.Add(idx);
                }
            }
        }

        public string GetScheduleChanges(int requestId)
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

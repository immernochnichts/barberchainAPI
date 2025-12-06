using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Security.Claims;

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
        public int UserId { get; set; }
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

            var barberId = await _context.Barbers.Where(b => b.AccountId == dto.UserId).Select(b => b.Id).FirstOrDefaultAsync();
            var requestDate = dto.SelectedDate;

            var alreadyExistingRequest = await _context.ScheduleRequests.Where(sr => sr.RequestDate == requestDate && sr.BarberId == barberId).FirstOrDefaultAsync();

            if (alreadyExistingRequest != null)
            {
                alreadyExistingRequest.AtuPattern = dto.AtuPattern;
                alreadyExistingRequest.Message = dto.Message;
                alreadyExistingRequest.Status = ScheduleRequestStatus.Pending;
                alreadyExistingRequest.OrderIdsToDecline = string.Join(" ", dto.OrdersToDecline.Select(o => o.Id.ToString()));
                await _context.SaveChangesAsync();
                return null!;
            }
            else
            {
                CreateReplaceSchedReqResult result = new CreateReplaceSchedReqResult()
                {
                    Request = new ScheduleRequest
                    {
                        BarberId = barberId,
                        RequestDate = requestDate,
                        AtuPattern = dto.AtuPattern,
                        Message = dto.Message,
                        Status = ScheduleRequestStatus.Pending,
                        OrderIdsToDecline = string.Join(" ", dto.OrdersToDecline.Select(o => o.Id.ToString()))
                    },
                    Error = null!
                };

                _context.ScheduleRequests.Add(result.Request);
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
    }
}

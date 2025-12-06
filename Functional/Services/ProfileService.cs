using barberchainAPI.Components.Pages;
using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;
using MudBlazor;
using System.Collections;
using System.Diagnostics;
using static barberchainAPI.Components.Pages.Profile;

namespace barberchainAPI.Functional.Services
{
    public class SubmitReviewDto
    {
        public int UserId { get; set; } = default!;
        public Barber Barber { get; set; } = default!;
        public ReviewModel ReviewModel { get; set; } = default!;
    }

    public class ProfileService : IProfileService
    {
        BarberchainDbContext _context;
        public ProfileService(BarberchainDbContext context)
        {
            _context = context;
        }

        public async Task<BitArray> GetAvailabilityForDayAsync(int barberId, DateTime? day)
        {
            var schedRequest = await _context.ScheduleRequests
                .Where(a => a.BarberId == barberId && a.RequestDate == DateOnly.FromDateTime(day.Value) && a.Status == ScheduleRequestStatus.Pending)
                .FirstOrDefaultAsync();

            var availability = schedRequest?.AtuPattern;

            if (availability != null) // first check schedule requests
            {
                foreach (var strId in schedRequest!.OrderIdsToDecline.Split(" ", StringSplitOptions.RemoveEmptyEntries))
                {
                    int orderId = int.Parse(strId);

                    var order = await _context.Orders.Include(o => o.OrderJobs).Where(o => o.Id == orderId).FirstOrDefaultAsync();
                    var declinedOrderJobs = order!.OrderJobs.Select(oj => oj.Job);
                    var orderDuration = declinedOrderJobs.Select(oj => (int)oj.DurationAtu).Sum();

                    int startIdx = order.AppointedTime.Hour * 4 + order.AppointedTime.Minute / 15;
                    int endIdx = startIdx + orderDuration - 1;

                    for (int i = startIdx; i <= endIdx; i++)
                    {
                        availability[i] = false;    // mark times booked by potentially declined orders as unavailable to avoid overlapping
                    }
                }

                return availability;
            }
            else
            {
                availability = await _context.BarberScheduleDays // then check schedule days
                    .Where(a => a.BarberId == barberId && a.Date == DateOnly.FromDateTime(day.Value))
                    .Select(a => a.AtuPattern)
                    .FirstOrDefaultAsync();

                return availability ?? new BitArray(Enumerable.Repeat<bool>(false, 96).ToArray());
            }
        }

        public async Task<Tuple<string, Severity>> SubmitReviewAsync(SubmitReviewDto dto)
        {
            if (!await _context.Orders.AnyAsync(o => o.BarberId == dto.Barber!.Id && o.AccountId == dto.UserId && o.Status == OrderStatus.Complete))
            {
                return Tuple.Create("Вы не можете оставить отзыв о сотруднике, если у вас нет с ним ни единого выполненного заказа", Severity.Info);
            }

            if (await _context.Reviews.AnyAsync(r => r.BarberId == dto.Barber!.Id && r.AccountId == dto.UserId))
            {
                dto.Barber!.ReviewSum += dto.ReviewModel.Score;
                dto.Barber!.ReviewCount += 1;

                var review = new Review
                {
                    Score = (short)dto.ReviewModel.Score,
                    Text = dto.ReviewModel.Text,
                    OrderId = dto.ReviewModel.OrderId == 0 ? null : dto.ReviewModel.OrderId,
                    AccountId = dto.UserId,
                    BarberId = dto.Barber.Id,
                    CreatedAt = DateTime.Now
                };

                // adding review to the db
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();

                // sending notification to the barber
                var not = new Notification
                {
                    Type = NotificationType.General,
                    Content = "Пользователь оставил новый отзыв о Вас:\n\n" + review.Text
                };

                _context.Notifications.Add(not);
                await _context.SaveChangesAsync();

                var acc_not = new AccountNotification
                {
                    AccountId = dto.Barber.AccountId!.Value,
                    NotificationId = not.Id
                };

                _context.AccountNotifications.Add(acc_not);
                await _context.SaveChangesAsync();

                dto.ReviewModel = new ReviewModel();
                return Tuple.Create("Отзыв успешно отправлен", Severity.Success);
            }
            
            return Tuple.Create("Вы уже оставили отзыв об этом сотруднике", Severity.Info);
        }
    }
}

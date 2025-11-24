using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace barberchainAPI.Functional
{
    public class OrderExpirationService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderExpirationService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BarberchainDbContext>();

                var now = DateTime.Now;

                var expiredOrders = await context.Orders
                    .Include(o => o.OrderJobs)
                        .ThenInclude(oj => oj.Job)
                    .Where(o => o.Status == OrderStatus.Pending &&
                                o.PendingUntil <= now)
                    .ToListAsync(stoppingToken);

                foreach (var order in expiredOrders)
                {
                    order.Status = OrderStatus.Declined;

                    var bscd = await context.BarberScheduleDays
                        .Where(d => d.Date == DateOnly.FromDateTime(order.AppointedTime))
                        .FirstOrDefaultAsync();

                    var newBits = new BitArray(bscd.AtuPattern);  // clone

                    int startIndex = order.AppointedTime.Hour * 4 + order.AppointedTime.Minute / 15;
                    int endIndex = startIndex + order.OrderJobs.Select(oj => (int)oj.Job.DurationAtu).Sum();
                    int i = startIndex;
                    while (i <= endIndex)
                    {
                        newBits[i] = true;
                        i++;
                    }

                    bscd.AtuPattern = newBits; // updating barber_schedule_day in the DB
                    await context.SaveChangesAsync();
                }

                await context.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}

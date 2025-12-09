using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace barberchainAPI.Functional
{
    public class OrderUpdaterService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OrderUpdaterService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await UpdateOrderStatuses();

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private async Task UpdateOrderStatuses()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BarberchainDbContext>();

            var now = DateTime.Now;

            var orders = await context.Orders
                .Include(o => o.OrderJobs)
                    .ThenInclude(oj => oj.Job)
                .Where(o => o.Status != OrderStatus.Complete && o.Status != OrderStatus.Declined)
                .ToListAsync();

            foreach (var order in orders)
            {
                var totalDurationMinutes = order.OrderJobs?.Sum(oj => oj.Job?.DurationAtu * 15 ?? 0) ?? 0;
                var endTime = order.AppointedTime.AddMinutes(totalDurationMinutes);

                if (order.Status != OrderStatus.Processing && now >= order.AppointedTime && now < endTime)
                {
                    order.Status = OrderStatus.Processing;
                }

                if (order.Status != OrderStatus.Complete && now >= endTime)
                {
                    order.Status = OrderStatus.Complete;
                    order.IsPaid = true;
                }
            }

            await context.SaveChangesAsync();
        }
    }
}

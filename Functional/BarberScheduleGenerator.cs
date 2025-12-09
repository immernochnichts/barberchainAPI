using barberchainAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace barberchainAPI.Functional
{
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading;
    using System.Threading.Tasks;

    public class BarberScheduleGenerator : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public BarberScheduleGenerator(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await GenerateSchedules();

                // wait until next day at 00:00
                var now = DateTime.Now;
                var nextRun = DateTime.Today.AddDays(1);
                var delay = nextRun - now;
                if (delay < TimeSpan.Zero)
                    delay = TimeSpan.Zero;

                await Task.Delay(delay, stoppingToken);
            }
        }

        private async Task GenerateSchedules()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<BarberchainDbContext>();

            var barbers = await context.Barbers.ToListAsync();
            var today = DateTime.Today;

            // rolling 7-day schedule starting from today
            for (int offset = 0; offset <= 7; offset++)
            {
                var date = today.AddDays(offset);

                if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                    continue;

                foreach (var barber in barbers)
                {
                    var exists = await context.BarberScheduleDays
                        .AnyAsync(s => s.BarberId == barber.Id && s.Date == DateOnly.FromDateTime(date));

                    if (!exists)
                    {
                        context.BarberScheduleDays.Add(new BarberScheduleDay
                        {
                            BarberId = barber.Id,
                            Date = DateOnly.FromDateTime(date),
                            AtuPattern = barber.Bshop.DefaultSchedule
                        });
                    }
                }
            }

            await context.SaveChangesAsync();
        }
    }

}

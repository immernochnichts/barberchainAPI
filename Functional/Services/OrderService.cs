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

            _context.BarberScheduleDays.Update(bsd); // randomly leaving this here for the edgecase where a guest's order is placed
            await _context.SaveChangesAsync();
        }

        ///<summary>
        ///  Order status has to be set outside of the method
        ///  Order must include OrderJobs and then include Job
        ///</summary>
        public async Task EraseOrderFromScheduleAsync(BarberScheduleDay bsd, Order order)
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

            var tracked = _context.BarberScheduleDays
                .Local
                .FirstOrDefault(x => x.Id == bsd.Id);

            if (tracked == null)
            {
                _context.BarberScheduleDays.Update(bsd);
            }

            await _context.SaveChangesAsync();
        }
    }
}

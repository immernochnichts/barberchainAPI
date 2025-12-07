using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public interface IOrderService
    {
        Task EraseOrderFromScheduleAsync(BarberScheduleDay bsd, Order order);
        Task AddOrderToScheduleAsync(BarberScheduleDay bsd, Order order);
    }
}

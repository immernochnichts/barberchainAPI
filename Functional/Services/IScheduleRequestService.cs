using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public interface IScheduleRequestService
    {
        Task ApproveRequestAsync(ScheduleRequest req);
        Task<List<ScheduleRequest>> LoadRequestsAsync(int managerId);
        Task NotifyBarberAsync(ScheduleRequest req);
    }
}

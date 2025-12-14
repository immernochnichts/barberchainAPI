using barberchainAPI.Data;

namespace barberchainAPI.Functional.Services
{
    public interface IScheduleRequestService
    {
        Task ApproveRequestAsync(int requestId);
        Task<List<ScheduleRequest>> LoadRequestsAsync(int managerId);
        Task NotifyBarberAsync(int requestId);
    }
}

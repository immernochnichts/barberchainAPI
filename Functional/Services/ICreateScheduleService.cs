using barberchainAPI.Data;
using System.Collections;

namespace barberchainAPI.Functional.Services
{
    public interface ICreateScheduleService
    {
        Task<BarberScheduleLoadResult> LoadBarberScheduleAsync(int barberId, DateOnly date);
        Task<CreateReplaceSchedReqResult> CreateOrReplaceScheduleRequest(CreateReplaceSchedDto dto);
        string GetScheduleChanges(int requestId);
        Task NotifyManagerAsync(Barber barber, DateTime selectedDate);
        Task CancelChangesAsync(CancelChangesDto dto);
    }
}

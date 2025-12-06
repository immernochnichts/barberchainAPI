using MudBlazor;
using System.Collections;

namespace barberchainAPI.Functional.Services
{
    public interface IProfileService
    {
        Task<BitArray> GetAvailabilityForDayAsync(int barberId, DateTime? day);

        Task<Tuple<string, Severity>> SubmitReviewAsync(SubmitReviewDto dto);
    }
}

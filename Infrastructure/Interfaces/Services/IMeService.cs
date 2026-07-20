using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IMeService
    {
        Task<MyProfileResponse> GetProfileAsync(Guid memberId);
        Task<HeartsDto> GetHeartsAsync(Guid memberId);
        Task<StreakDto> GetStreakAsync(Guid memberId);
        Task UpdateDailyGoalAsync(Guid memberId, int goalMinutes);
        Task<List<SectionPathResponse>> GetMyLearningPathAsync(Guid memberId);
    }
}

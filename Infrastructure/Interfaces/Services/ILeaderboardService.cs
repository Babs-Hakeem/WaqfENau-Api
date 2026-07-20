using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface ILeaderboardService
    {
        Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid memberId, string scope);
    }
}

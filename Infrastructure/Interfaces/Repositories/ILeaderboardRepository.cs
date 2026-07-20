using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface ILeaderboardRepository : IBaseRepository<LeaderboardEntry>
    {
        Task<IEnumerable<LeaderboardEntry>> GetByScopeAsync(string scope, Guid? branchId = null, int top = 50);
        Task<LeaderboardEntry?> GetByMemberAndScopeAsync(Guid memberId, string scope);
    }
}

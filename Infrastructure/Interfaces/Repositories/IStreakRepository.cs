using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface IStreakRepository : IBaseRepository<Streak>
    {
        Task<Streak?> GetByMemberIdAsync(Guid memberId);
        Task<IEnumerable<Streak>> GetInactiveStreaksAsync(int daysInactive);
    }
}

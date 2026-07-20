using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class StreakRepository : BaseRepository<Streak>, IStreakRepository
    {
        public StreakRepository(WaqfENauContext context) : base(context) { }

        public async Task<Streak?> GetByMemberIdAsync(Guid memberId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(s => s.MemberId == memberId);
        }

        public async Task<IEnumerable<Streak>> GetInactiveStreaksAsync(int daysInactive)
        {
            var cutoffDate = DateTime.UtcNow.Date.AddDays(-daysInactive);
            return await _dbSet
                .Where(s => s.LastActivityDate < cutoffDate && s.CurrentStreak > 0)
                .Include(s => s.Member)
                .ToListAsync();
        }
    }
}

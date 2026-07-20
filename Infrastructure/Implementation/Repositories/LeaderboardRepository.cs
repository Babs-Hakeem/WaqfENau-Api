using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;
namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class LeaderboardRepository : BaseRepository<LeaderboardEntry>, ILeaderboardRepository
    {
        public LeaderboardRepository(WaqfENauContext context) : base(context) { }

        public async Task<IEnumerable<LeaderboardEntry>> GetByScopeAsync(string scope, Guid? branchId = null, int top = 50)
        {
            var query = _dbSet
                .Where(le => le.Scope == scope)
                .AsQueryable();

            if (branchId.HasValue)
                query = query.Where(le => le.BranchId == branchId);

            return await query
                .OrderBy(le => le.Rank)
                .Take(top)
                .Include(le => le.Member)
                .ToListAsync();
        }

        public async Task<LeaderboardEntry?> GetByMemberAndScopeAsync(Guid memberId, string scope)
        {
            return await _dbSet
                .FirstOrDefaultAsync(le => le.MemberId == memberId && le.Scope == scope);
        }
    }
}

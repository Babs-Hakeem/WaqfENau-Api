using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class ProgressRepository : BaseRepository<MemberProgress>, IProgressRepository
    {
        public ProgressRepository(WaqfENauContext context) : base(context) { }

        public async Task<MemberProgress?> GetByMemberAndLessonAsync(Guid memberId, Guid lessonId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(p => p.MemberId == memberId && p.LessonId == lessonId);
        }

        public async Task<IEnumerable<MemberProgress>> GetByMemberAsync(Guid memberId)
        {
            return await _dbSet
                .Where(p => p.MemberId == memberId)
                .ToListAsync();
        }

        public async Task<int> CountCompletedByMemberAsync(Guid memberId)
        {
            return await _dbSet
                .CountAsync(p => p.MemberId == memberId && p.IsCompleted);
        }
    }
}

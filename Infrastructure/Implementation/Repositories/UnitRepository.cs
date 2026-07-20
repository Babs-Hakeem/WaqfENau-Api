using Microsoft.EntityFrameworkCore;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class UnitRepository : BaseRepository<Unit>, IUnitRepository
    {
        public UnitRepository(WaqfENauContext context) : base(context) { }

        public async Task<IEnumerable<Unit>> GetBySectionIdAsync(Guid sectionId, bool publishedOnly = true)
        {
            var query = _dbSet.Where(u => u.SectionId == sectionId && u.IsActive);

            if (publishedOnly)
                query = query.Where(u => u.IsPublished);

            return await query.OrderBy(u => u.OrderIndex).ToListAsync();
        }

        public async Task<Unit?> GetByIdWithLessonsAsync(Guid id)
        {
            return await _dbSet
                .Include(u => u.Lessons.OrderBy(l => l.OrderIndex))
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class SectionRepository : BaseRepository<Section>, ISectionRepository
    {
        public SectionRepository(WaqfENauContext context) : base(context) { }

        public async Task<IEnumerable<Section>> GetByAgeGroupAsync(AgeGroup ageGroup, bool publishedOnly = true)
        {
            var query = _dbSet.Where(s => s.AgeGroup == ageGroup && s.IsActive);

            if (publishedOnly)
                query = query.Where(s => s.IsPublished);

            return await query.OrderBy(s => s.OrderIndex).ToListAsync();
        }

        public async Task<Section?> GetByIdWithUnitsAsync(Guid id)
        {
            return await _dbSet
                .Include(s => s.Units.OrderBy(u => u.OrderIndex))
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<Section>> GetAllWithUnitCountAsync()
        {
            return await _dbSet
                .Include(s => s.Units)
                .OrderBy(s => s.AgeGroup)
                .ThenBy(s => s.OrderIndex)
                .ToListAsync();
        }
    }
}

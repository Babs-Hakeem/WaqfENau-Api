using Microsoft.EntityFrameworkCore;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class LessonRepository : BaseRepository<Lesson>, ILessonRepository
    {
        public LessonRepository(WaqfENauContext context) : base(context) { }

        public async Task<IEnumerable<Lesson>> GetByUnitIdAsync(Guid unitId)
        {
            return await _dbSet
                .Where(l => l.UnitId == unitId && l.IsActive)
                .OrderBy(l => l.OrderIndex)
                .ToListAsync();
        }

        public async Task<Lesson?> GetByIdWithExercisesAsync(Guid id)
        {
            return await _dbSet
                .Include(l => l.Exercises.OrderBy(e => e.OrderIndex))
                    .ThenInclude(e => e.Options.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<IEnumerable<Lesson>> GetActiveLessonsAsync()
        {
            return await _dbSet
                .Where(l => l.IsActive)
                .ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class ExerciseRepository : BaseRepository<Exercise>, IExerciseRepository
    {
        public ExerciseRepository(WaqfENauContext context) : base(context) { }

        public async Task<IEnumerable<Exercise>> GetByLessonIdAsync(Guid lessonId)
        {
            return await _dbSet
                .Where(e => e.LessonId == lessonId)
                .Include(e => e.Options.OrderBy(o => o.OrderIndex))
                .OrderBy(e => e.OrderIndex)
                .ToListAsync();
        }

        public async Task<Exercise?> GetByIdWithOptionsAsync(Guid id)
        {
            return await _dbSet
                .Include(e => e.Options.OrderBy(o => o.OrderIndex))
                .FirstOrDefaultAsync(e => e.Id == id);
        }
    }
}

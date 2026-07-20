using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface IExerciseRepository : IBaseRepository<Exercise>
    {
        Task<IEnumerable<Exercise>> GetByLessonIdAsync(Guid lessonId);
        Task<Exercise?> GetByIdWithOptionsAsync(Guid id);
    }
}

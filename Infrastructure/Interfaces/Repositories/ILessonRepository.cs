using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface ILessonRepository : IBaseRepository<Lesson>
    {
        Task<IEnumerable<Lesson>> GetByUnitIdAsync(Guid unitId);
        Task<Lesson?> GetByIdWithExercisesAsync(Guid id);
        Task<IEnumerable<Lesson>> GetActiveLessonsAsync();
    }
}

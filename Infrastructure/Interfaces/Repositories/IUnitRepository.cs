using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface IUnitRepository : IBaseRepository<Unit>
    {
        Task<IEnumerable<Unit>> GetBySectionIdAsync(Guid sectionId, bool publishedOnly = true);
        Task<Unit?> GetByIdWithLessonsAsync(Guid id);
    }
}

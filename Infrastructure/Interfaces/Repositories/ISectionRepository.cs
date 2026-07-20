using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface ISectionRepository : IBaseRepository<Section>
    {
        Task<IEnumerable<Section>> GetByAgeGroupAsync(AgeGroup ageGroup, bool publishedOnly = true);
        Task<Section?> GetByIdWithUnitsAsync(Guid id);
        Task<IEnumerable<Section>> GetAllWithUnitCountAsync();
    }
}

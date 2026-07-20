using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories
{
    public interface IProgressRepository : IBaseRepository<MemberProgress>
    {
        Task<MemberProgress?> GetByMemberAndLessonAsync(Guid memberId, Guid lessonId);
        Task<IEnumerable<MemberProgress>> GetByMemberAsync(Guid memberId);
        Task<int> CountCompletedByMemberAsync(Guid memberId);
    }
}

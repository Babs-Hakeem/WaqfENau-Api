using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories;

public interface IMemberRepository : IBaseRepository<Member>
{
    Task<Member?> GetByEmailAsync(string email);
    Task<Member?> GetByIdWithDetailsAsync(Guid id);
    Task<IEnumerable<Member>> GetByBranchAsync(Guid branchId);
}
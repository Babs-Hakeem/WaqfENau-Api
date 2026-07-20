using Microsoft.EntityFrameworkCore;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories;

public class MemberRepository : BaseRepository<Member>, IMemberRepository
{
    public MemberRepository(WaqfENauContext context) : base(context) { }

    public async Task<Member?> GetByEmailAsync(string email)
    {
        return await _dbSet.FirstOrDefaultAsync(m => m.Email == email);
    }

    public async Task<Member?> GetByIdWithDetailsAsync(Guid id)
    {
        return await _dbSet
            .Include(m => m.Branch)
            .Include(m => m.Streak)
            .Include(m => m.Progresses)
            .Include(m => m.MemberAchievements)
            .ThenInclude(ma => ma.Achievement)
            .FirstOrDefaultAsync(m => m.Id == id);
    }

    public async Task<IEnumerable<Member>> GetByBranchAsync(Guid branchId)
    {
        return await _dbSet
            .Where(m => m.BranchId == branchId)
            .ToListAsync();
    }
}
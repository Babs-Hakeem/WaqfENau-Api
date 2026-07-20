using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly IUnitOfWork _unitOfWork;

        public LeaderboardService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<LeaderboardEntryDto>> GetLeaderboardAsync(Guid memberId, string scope)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId);
            if (member == null)
                throw new Exception("Member not found");

            var entries = await _unitOfWork.Leaderboard.GetByScopeAsync(
                scope,
                scope == "Branch" ? member.BranchId : null,
                50
            );

            // Recalculate ranks if stale
            var entryList = entries.ToList();
            for (int i = 0; i < entryList.Count; i++)
            {
                entryList[i].Rank = i + 1;
            }

            return entryList.Select(e => new LeaderboardEntryDto
            {
                Rank = e.Rank,
                MemberName = e.Member != null ? $"{e.Member.FirstName} {e.Member.LastName}" : "Unknown",
                BranchName = e.Member?.Branch?.Name ?? string.Empty,
                TotalXp = e.TotalXp,
                CurrentStreak = e.CurrentStreak,
                LessonsCompleted = e.LessonsCompleted,
                CurrentLevel = e.Member?.CurrentLevel ?? 1
            }).ToList();
        }
    }

}

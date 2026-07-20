using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class GamificationService : IGamificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public GamificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<string>> CheckAchievementsAsync(Guid memberId)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId);
            if (member == null) return new List<string>();

            var unlocked = new List<string>();
            var allAchievements = await _unitOfWork.Repository<Achievement>().GetAllAsync();
            var earnedIds = member.MemberAchievements.Select(ma => ma.AchievementId).ToHashSet();
            var lessonsCompleted = await _unitOfWork.Progresses.CountCompletedByMemberAsync(memberId);

            // Count completed units (all lessons in unit must be completed)
            int unitsCompleted = await CountCompletedUnitsAsync(memberId, member.AgeGroup);

            foreach (var achievement in allAchievements.Where(a => !earnedIds.Contains(a.Id)))
            {
                bool shouldUnlock = achievement.ConditionType switch
                {
                    "LessonsCompleted" => lessonsCompleted >= achievement.ConditionValue,
                    "StreakCount"      => member.Streak?.CurrentStreak >= achievement.ConditionValue,
                    "TotalXp"         => member.TotalXp >= achievement.ConditionValue,
                    "UnitsCompleted"  => unitsCompleted >= achievement.ConditionValue,
                    _                 => false
                };

                if (shouldUnlock)
                {
                    await _unitOfWork.Repository<MemberAchievement>().AddAsync(new MemberAchievement
                    {
                        MemberId = memberId,
                        AchievementId = achievement.Id
                    });

                    // Bonus XP for unlocking the achievement
                    member.TotalXp += achievement.XpReward;
                    unlocked.Add(achievement.Name);
                }
            }

            if (unlocked.Any())
            {
                _unitOfWork.Members.Update(member);
                await _unitOfWork.SaveChangesAsync();
            }

            return unlocked;
        }

        public async Task AwardXpAsync(Guid memberId, int amount, string reason, Guid? referenceId = null)
        {
            await _unitOfWork.Repository<XpTransaction>().AddAsync(new XpTransaction
            {
                MemberId = memberId,
                Amount = amount,
                Reason = reason,
                ReferenceId = referenceId
            });

            var member = await _unitOfWork.Members.GetByIdAsync(memberId);
            if (member != null)
            {
                member.TotalXp += amount;
                member.CurrentLevel = (int)Math.Floor(Math.Sqrt(member.TotalXp / 100.0)) + 1;
                _unitOfWork.Members.Update(member);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateLeaderboardAsync(Guid memberId)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId);
            if (member == null) return;

            var lessonsCompleted = await _unitOfWork.Progresses.CountCompletedByMemberAsync(memberId);

            await UpsertLeaderboardEntryAsync(
                memberId, "Branch", member.BranchId,
                member.TotalXp, member.Streak?.CurrentStreak ?? 0, lessonsCompleted);

            await UpsertLeaderboardEntryAsync(
                memberId, "National", null,
                member.TotalXp, member.Streak?.CurrentStreak ?? 0, lessonsCompleted);

            await _unitOfWork.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────────

        private async Task UpsertLeaderboardEntryAsync(
            Guid memberId, string scope, Guid? branchId,
            int totalXp, int currentStreak, int lessonsCompleted)
        {
            var entry = await _unitOfWork.Leaderboard.GetByMemberAndScopeAsync(memberId, scope);

            if (entry == null)
            {
                entry = new LeaderboardEntry
                {
                    MemberId = memberId,
                    Scope = scope,
                    BranchId = branchId,
                    WeeklyXp = 0
                };
                await _unitOfWork.Leaderboard.AddAsync(entry);
            }

            // Track how much XP was gained in this update for weekly total
            int xpGained = Math.Max(0, totalXp - entry.TotalXp);
            entry.WeeklyXp += xpGained;

            entry.TotalXp = totalXp;
            entry.CurrentStreak = currentStreak;
            entry.LessonsCompleted = lessonsCompleted;
            entry.LastUpdated = DateTime.UtcNow;

            _unitOfWork.Leaderboard.Update(entry);
        }

        private async Task<int> CountCompletedUnitsAsync(Guid memberId, Models.Enums.AgeGroup ageGroup)
        {
            var completedLessonIds = (await _unitOfWork.Progresses
                .GetByMemberAsync(memberId))
                .Where(p => p.IsCompleted)
                .Select(p => p.LessonId)
                .ToHashSet();

            int completedUnits = 0;

            var sections = await _unitOfWork.Sections.GetByAgeGroupAsync(ageGroup, publishedOnly: true);
            foreach (var section in sections)
            {
                var units = await _unitOfWork.Units.GetBySectionIdAsync(section.Id, publishedOnly: true);
                foreach (var unit in units)
                {
                    var lessons = (await _unitOfWork.Lessons.GetByUnitIdAsync(unit.Id)).ToList();
                    if (lessons.Any() && lessons.All(l => completedLessonIds.Contains(l.Id)))
                        completedUnits++;
                }
            }

            return completedUnits;
        }
    }
}

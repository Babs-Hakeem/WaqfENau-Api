using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class MeService : IMeService
    {
        private readonly IUnitOfWork _unitOfWork;

        public MeService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<MyProfileResponse> GetProfileAsync(Guid memberId)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId)
                ?? throw new Exception("Member not found");

            var lessonsCompleted = await _unitOfWork.Progresses.CountCompletedByMemberAsync(memberId);
            var hearts = await GetOrCreateHeartsAsync(memberId);
            RefillHeartsIfDue(hearts);

            return new MyProfileResponse
            {
                Id = member.Id,
                FirstName = member.FirstName,
                LastName = member.LastName,
                Email = member.Email,
                AgeGroup = member.AgeGroup.ToString(),
                Role = member.Role.ToString(),
                BranchName = member.Branch?.Name ?? string.Empty,
                TotalXp = member.TotalXp,
                CurrentLevel = member.CurrentLevel,
                DailyGoalMinutes = member.DailyGoalMinutes,
                LessonsCompleted = lessonsCompleted,
                Streak = MapStreak(member.Streak),
                Hearts = MapHearts(hearts)
            };
        }

        public async Task<HeartsDto> GetHeartsAsync(Guid memberId)
        {
            var hearts = await GetOrCreateHeartsAsync(memberId);
            RefillHeartsIfDue(hearts);
            return MapHearts(hearts);
        }

        public async Task<StreakDto> GetStreakAsync(Guid memberId)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId)
                ?? throw new Exception("Member not found");

            return MapStreak(member.Streak);
        }

        public async Task UpdateDailyGoalAsync(Guid memberId, int goalMinutes)
        {
            var allowed = new[] { 5, 10, 15, 20 };
            if (!allowed.Contains(goalMinutes))
                throw new Exception("Daily goal must be 5, 10, 15, or 20 minutes");

            var member = await _unitOfWork.Members.GetByIdAsync(memberId)
                ?? throw new Exception("Member not found");

            member.DailyGoalMinutes = goalMinutes;
            member.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Members.Update(member);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<SectionPathResponse>> GetMyLearningPathAsync(Guid memberId)
        {
            var member = await _unitOfWork.Members.GetByIdAsync(memberId)
                ?? throw new Exception("Member not found");

            // Get all published sections for the member's age group
            var sections = await _unitOfWork.Sections.GetByAgeGroupAsync(member.AgeGroup, publishedOnly: true);

            // Get all member progress in one query (avoid N+1)
            var allProgress = (await _unitOfWork.Progresses.GetByMemberAsync(memberId)).ToList();
            var completedLessonIds = allProgress.Where(p => p.IsCompleted).Select(p => p.LessonId).ToHashSet();

            var result = new List<SectionPathResponse>();
            bool previousUnitCompleted = true;

            foreach (var section in sections.OrderBy(s => s.OrderIndex))
            {
                var units = await _unitOfWork.Units.GetBySectionIdAsync(section.Id, publishedOnly: true);
                var unitResponses = new List<UnitPathResponse>();

                foreach (var unit in units.OrderBy(u => u.OrderIndex))
                {
                    var lessons = (await _unitOfWork.Lessons.GetByUnitIdAsync(unit.Id)).ToList();
                    int totalLessons = lessons.Count;
                    int completedLessons = lessons.Count(l => completedLessonIds.Contains(l.Id));
                    bool isCompleted = totalLessons > 0 && completedLessons == totalLessons;
                    int progressPercent = totalLessons > 0
                        ? (int)Math.Round(completedLessons / (double)totalLessons * 100)
                        : 0;

                    unitResponses.Add(new UnitPathResponse
                    {
                        Id = unit.Id,
                        Title = unit.Title,
                        Description = unit.Description,
                        GuidebookContent = unit.GuidebookContent,
                        Category = unit.Category.ToString(),
                        OrderIndex = unit.OrderIndex,
                        XpReward = unit.XpReward,
                        TotalLessons = totalLessons,
                        CompletedLessons = completedLessons,
                        ProgressPercent = progressPercent,
                        IsUnlocked = previousUnitCompleted,
                        IsCompleted = isCompleted
                    });

                    previousUnitCompleted = isCompleted;
                }

                result.Add(new SectionPathResponse
                {
                    Id = section.Id,
                    Title = section.Title,
                    Description = section.Description,
                    AgeGroup = section.AgeGroup.ToString(),
                    OrderIndex = section.OrderIndex,
                    Units = unitResponses
                });
            }

            return result;
        }

        // ── Helpers ──────────────────────────────────────────────────

        private async Task<Hearts> GetOrCreateHeartsAsync(Guid memberId)
        {
            var hearts = (await _unitOfWork.Repository<Hearts>()
                .FindAsync(h => h.MemberId == memberId)).FirstOrDefault();

            if (hearts == null)
            {
                hearts = new Hearts { MemberId = memberId, Current = Hearts.Max };
                await _unitOfWork.Repository<Hearts>().AddAsync(hearts);
                await _unitOfWork.SaveChangesAsync();
            }

            return hearts;
        }

        private static void RefillHeartsIfDue(Hearts hearts)
        {
            if (hearts.IsFull || hearts.NextRefillAt == null) return;

            var now = DateTime.UtcNow;
            while (hearts.NextRefillAt != null && hearts.NextRefillAt <= now && hearts.Current < Hearts.Max)
            {
                hearts.Current += 1;
                hearts.NextRefillAt = hearts.Current < Hearts.Max
                    ? hearts.NextRefillAt.Value.AddMinutes(Hearts.RefillMinutes)
                    : null;
            }
        }

        private static StreakDto MapStreak(Streak? streak) => new()
        {
            CurrentStreak = streak?.CurrentStreak ?? 0,
            LongestStreak = streak?.LongestStreak ?? 0,
            FreezesAvailable = streak?.FreezesAvailable ?? 0,
            LastActivityDate = streak?.LastActivityDate,
            StudiedToday = streak?.LastActivityDate?.Date == DateTime.UtcNow.Date
        };

        private static HeartsDto MapHearts(Hearts hearts) => new()
        {
            Current = hearts.Current,
            Max = Hearts.Max,
            NextRefillAt = hearts.NextRefillAt,
            IsFull = hearts.IsFull
        };
    }
}

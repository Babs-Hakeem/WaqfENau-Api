using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class LessonService : ILessonService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IGamificationService _gamificationService;

        public LessonService(IUnitOfWork unitOfWork, IGamificationService gamificationService)
        {
            _unitOfWork = unitOfWork;
            _gamificationService = gamificationService;
        }

        public async Task<List<LessonResponse>> GetLessonsByUnitAsync(Guid memberId, Guid unitId)
        {
            var lessons = (await _unitOfWork.Lessons.GetByUnitIdAsync(unitId)).OrderBy(l => l.OrderIndex).ToList();
            var progress = await _unitOfWork.Progresses.GetByMemberAsync(memberId);
            var progressDict = progress.ToDictionary(p => p.LessonId, p => p);

            var result = new List<LessonResponse>();
            bool previousCompleted = true; // first lesson is always unlocked

            foreach (var lesson in lessons)
            {
                var hasProgress = progressDict.TryGetValue(lesson.Id, out var p);

                result.Add(new LessonResponse
                {
                    Id = lesson.Id,
                    Title = lesson.Title,
                    Description = lesson.Description,
                    XpReward = lesson.XpReward,
                    EstimatedMinutes = lesson.EstimatedMinutes,
                    OrderIndex = lesson.OrderIndex,
                    IsCompleted = hasProgress && p!.IsCompleted,
                    Score = hasProgress ? p!.Score : null,
                    IsLocked = !previousCompleted
                });

                previousCompleted = hasProgress && p!.IsCompleted;
            }

            return result;
        }

        public async Task<LessonDetailResponse?> GetLessonDetailAsync(Guid memberId, Guid lessonId)
        {
            var lesson = await _unitOfWork.Lessons.GetByIdWithExercisesAsync(lessonId);
            if (lesson == null) return null;

            return new LessonDetailResponse
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                XpReward = lesson.XpReward,
                EstimatedMinutes = lesson.EstimatedMinutes,
                Exercises = lesson.Exercises
                    .OrderBy(e => e.OrderIndex)
                    .Select(e => new ExerciseDto
                    {
                        Id = e.Id,
                        OrderIndex = e.OrderIndex,
                        Type = e.Type.ToString(),
                        Prompt = e.Prompt,
                        ExplanationText = e.ExplanationText,
                        AudioUrl = e.AudioUrl,
                        ImageUrl = e.ImageUrl,
                        SentenceTemplate = e.SentenceTemplate,
                        XpReward = e.XpReward,
                        Options = e.Options
                            .OrderBy(o => o.OrderIndex)
                            .Select(o => new ExerciseOptionDto
                            {
                                Id = o.Id,
                                Text = o.Text,
                                TextArabic = o.TextArabic,
                                OrderIndex = o.OrderIndex,
                                MatchGroupId = o.MatchGroupId
                            }).ToList()
                    }).ToList()
            };
        }

        public async Task<SubmitExerciseAnswerResponse> SubmitExerciseAnswerAsync(Guid memberId, SubmitExerciseAnswerRequest request)
        {
            var exercise = await _unitOfWork.Exercises.GetByIdWithOptionsAsync(request.ExerciseId)
                ?? throw new Exception("Exercise not found");

            var hearts = await GetOrCreateHeartsAsync(memberId);
            RefillHeartsIfDue(hearts);

            bool isCorrect = EvaluateAnswer(exercise, request.Answer);
            int xpEarned = 0;

            if (isCorrect)
            {
                xpEarned = exercise.XpReward;
            }
            else if (hearts.Current > 0)
            {
                hearts.Current -= 1;
                if (hearts.NextRefillAt == null)
                    hearts.NextRefillAt = DateTime.UtcNow.AddMinutes(Hearts.RefillMinutes);
            }

            var attempt = new ExerciseAttempt
            {
                MemberId = memberId,
                ExerciseId = exercise.Id,
                AnswerGiven = request.Answer,
                IsCorrect = isCorrect,
                HeartUsed = !isCorrect
            };

            await _unitOfWork.Repository<ExerciseAttempt>().AddAsync(attempt);
            _unitOfWork.Repository<Hearts>().Update(hearts);
            await _unitOfWork.SaveChangesAsync();

            string? correctText = null;
            if (!isCorrect)
            {
                var correctOption = exercise.Options.FirstOrDefault(o => o.IsCorrect);
                correctText = correctOption?.Text;
            }

            return new SubmitExerciseAnswerResponse
            {
                IsCorrect = isCorrect,
                XpEarned = xpEarned,
                HeartsRemaining = hearts.Current,
                CorrectAnswerText = correctText
            };
        }

        public async Task<CompleteLessonResponse> CompleteLessonAsync(Guid memberId, CompleteLessonRequest request)
        {
            var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(memberId)
                ?? throw new Exception("Member not found");

            var lesson = await _unitOfWork.Lessons.GetByIdWithExercisesAsync(request.LessonId)
                ?? throw new Exception("Lesson not found");

            var existingProgress = await _unitOfWork.Progresses.GetByMemberAndLessonAsync(memberId, request.LessonId);

            // Calculate score from attempts on this lesson's exercises
            var exerciseIds = lesson.Exercises.Select(e => e.Id).ToHashSet();
            var allAttempts = await _unitOfWork.Repository<ExerciseAttempt>()
                .FindAsync(a => a.MemberId == memberId && exerciseIds.Contains(a.ExerciseId));

            var firstAttempts = allAttempts
                .GroupBy(a => a.ExerciseId)
                .Select(g => g.OrderBy(a => a.AttemptedAt).First())
                .ToList();

            int totalExercises = lesson.Exercises.Count;
            int correctFirstTry = firstAttempts.Count(a => a.IsCorrect);
            int score = totalExercises > 0 ? (int)Math.Round(correctFirstTry / (double)totalExercises * 100) : 100;

            if (existingProgress != null && existingProgress.IsCompleted)
            {
                existingProgress.TimesReplayed += 1;
                existingProgress.Score = Math.Max(existingProgress.Score, score);
                // NOTE: no explicit .Update() call — existingProgress was loaded through
                // a tracked query, so EF's change tracker already sees these mutations
                // and will generate the correct UPDATE on SaveChangesAsync().
                await _unitOfWork.SaveChangesAsync();

                return new CompleteLessonResponse
                {
                    Success = true,
                    XpEarned = 0,
                    TotalXp = member.TotalXp,
                    CurrentLevel = member.CurrentLevel,
                    CurrentStreak = member.Streak?.CurrentStreak ?? 0,
                    Score = score,
                    UnlockedAchievements = new List<string>(),
                    Message = "Lesson replayed. No new XP awarded since it was already completed."
                };
            }

            var xpEarned = lesson.XpReward;
            if (score >= 90) xpEarned += 10;
            else if (score >= 70) xpEarned += 5;

            var progress = new MemberProgress
            {
                MemberId = memberId,
                LessonId = request.LessonId,
                IsCompleted = true,
                CompletedAt = DateTime.UtcNow,
                Score = score,
                XpEarned = xpEarned
            };

            await _unitOfWork.Progresses.AddAsync(progress);

            member.TotalXp += xpEarned;
            member.LastActiveDate = DateTime.UtcNow;
            member.CurrentLevel = CalculateLevel(member.TotalXp);

            UpdateStreak(member);

            // NOTE: no explicit .Update(member) call here. `member` is already tracked
            // (loaded via GetByIdWithDetailsAsync, which Includes Progresses), so EF's
            // change tracker already sees the mutations above and will generate the
            // right UPDATE for it on SaveChangesAsync(). Calling Update(member) was
            // actively harmful here: it walks the whole reachable graph, and the new
            // `progress` we just added above (which has a non-empty, client-generated
            // Id from BaseEntity, and gets auto-linked into member.Progresses via EF's
            // relationship fixup) could get its state flipped from Added to Modified —
            // producing an UPDATE for a row that doesn't exist yet, which matches zero
            // rows and throws exactly the concurrency exception seen in testing.
            await _unitOfWork.SaveChangesAsync();

            var unlockedAchievements = await _gamificationService.CheckAchievementsAsync(memberId);
            await _gamificationService.UpdateLeaderboardAsync(memberId);

            var nextLesson = (await _unitOfWork.Lessons.GetByUnitIdAsync(lesson.UnitId))
                .Where(l => l.OrderIndex > lesson.OrderIndex)
                .OrderBy(l => l.OrderIndex)
                .Select(l => l.Title)
                .FirstOrDefault();

            return new CompleteLessonResponse
            {
                Success = true,
                XpEarned = xpEarned,
                TotalXp = member.TotalXp,
                CurrentLevel = member.CurrentLevel,
                CurrentStreak = member.Streak?.CurrentStreak ?? 0,
                Score = score,
                UnlockedAchievements = unlockedAchievements,
                NextLessonTitle = nextLesson
            };
        }

        // ── Helpers ──────────────────────────────────────────────────

        private async Task<Hearts> GetOrCreateHeartsAsync(Guid memberId)
        {
            var hearts = (await _unitOfWork.Repository<Hearts>().FindAsync(h => h.MemberId == memberId)).FirstOrDefault();
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

        private static bool EvaluateAnswer(Exercise exercise, string answer)
        {
            switch (exercise.Type)
            {
                case Models.Enums.ExerciseType.InfoCard:
                    return true; // info cards always "pass" on continue

                case Models.Enums.ExerciseType.MultipleChoice:
                case Models.Enums.ExerciseType.TrueFalse:
                    if (!Guid.TryParse(answer, out var optionId)) return false;
                    var selected = exercise.Options.FirstOrDefault(o => o.Id == optionId);
                    return selected?.IsCorrect == true;

                case Models.Enums.ExerciseType.FillBlank:
                    var correctFill = exercise.Options.FirstOrDefault(o => o.IsCorrect);
                    return correctFill != null &&
                           string.Equals(correctFill.Text.Trim(), answer.Trim(), StringComparison.OrdinalIgnoreCase);

                case Models.Enums.ExerciseType.Arrange:
                    var correctOrder = exercise.Options.OrderBy(o => o.OrderIndex).Select(o => o.Id.ToString());
                    var givenOrder = answer.Split(',', StringSplitOptions.TrimEntries);
                    return correctOrder.SequenceEqual(givenOrder);

                case Models.Enums.ExerciseType.Match:
                    // answer format: "leftId:rightId,leftId:rightId,..."
                    var pairs = answer.Split(',', StringSplitOptions.TrimEntries);
                    foreach (var pair in pairs)
                    {
                        var parts = pair.Split(':');
                        if (parts.Length != 2) return false;
                        if (!Guid.TryParse(parts[0], out var leftId) || !Guid.TryParse(parts[1], out var rightId))
                            return false;

                        var left = exercise.Options.FirstOrDefault(o => o.Id == leftId);
                        var right = exercise.Options.FirstOrDefault(o => o.Id == rightId);
                        if (left == null || right == null) return false;
                        if (left.MatchGroupId == null || left.MatchGroupId != right.MatchGroupId) return false;
                    }
                    return true;

                default:
                    return false;
            }
        }

        private static void UpdateStreak(Member member)
        {
            if (member.Streak == null) return;

            var today = DateTime.UtcNow.Date;
            var yesterday = today.AddDays(-1);

            if (member.Streak.LastActivityDate?.Date == today)
            {
                // already counted today, no change
            }
            else if (member.Streak.LastActivityDate?.Date == yesterday)
            {
                member.Streak.CurrentStreak++;
                if (member.Streak.CurrentStreak > member.Streak.LongestStreak)
                    member.Streak.LongestStreak = member.Streak.CurrentStreak;
            }
            else
            {
                member.Streak.CurrentStreak = 1;
            }

            member.Streak.LastActivityDate = DateTime.UtcNow;
            member.Streak.FreezeUsedToday = false;
        }

        private static int CalculateLevel(int totalXp)
        {
            return (int)Math.Floor(Math.Sqrt(totalXp / 100.0)) + 1;
        }
    }
}

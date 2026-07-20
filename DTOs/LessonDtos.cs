using System.ComponentModel.DataAnnotations;

namespace WaqfENau.Api.DTOs
{
    public class LessonResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int EstimatedMinutes { get; set; }
        public int OrderIndex { get; set; }
        public bool IsCompleted { get; set; }
        public int? Score { get; set; }
        public bool IsLocked { get; set; }
    }

    public class LessonDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int XpReward { get; set; }
        public int EstimatedMinutes { get; set; }
        public List<ExerciseDto> Exercises { get; set; } = new();
    }

    public class ExerciseDto
    {
        public Guid Id { get; set; }
        public int OrderIndex { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Prompt { get; set; } = string.Empty;
        public string? ExplanationText { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? SentenceTemplate { get; set; }
        public int XpReward { get; set; }
        public List<ExerciseOptionDto> Options { get; set; } = new();
    }

    public class ExerciseOptionDto
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? TextArabic { get; set; }
        public int OrderIndex { get; set; }
        public int? MatchGroupId { get; set; }
        // NOTE: IsCorrect is intentionally NOT exposed to students here.
        // Correctness is validated server-side when they submit an answer.
    }

    public class SubmitExerciseAnswerRequest
    {
        [Required]
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// For MCQ/TrueFalse: the selected ExerciseOption Id (as string).
        /// For FillBlank: the typed text.
        /// For Arrange: comma-separated ExerciseOption Ids in chosen order.
        /// For Match: comma-separated "leftId:rightId" pairs.
        /// </summary>
        [Required]
        public string Answer { get; set; } = string.Empty;
    }

    public class SubmitExerciseAnswerResponse
    {
        public bool IsCorrect { get; set; }
        public int XpEarned { get; set; }
        public int HeartsRemaining { get; set; }
        public string? CorrectAnswerText { get; set; }
    }

    public class CompleteLessonRequest
    {
        [Required]
        public Guid LessonId { get; set; }
    }

    public class CompleteLessonResponse
    {
        public bool Success { get; set; }
        public int XpEarned { get; set; }
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentStreak { get; set; }
        public int Score { get; set; }
        public List<string> UnlockedAchievements { get; set; } = new();
        public string? NextLessonTitle { get; set; }
        public string? Message { get; set; }
    }
}

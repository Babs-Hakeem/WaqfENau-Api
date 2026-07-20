using System.ComponentModel.DataAnnotations;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.DTOs
{
    // ── SECTION ──────────────────────────────────────────────────────

    public class CreateSectionRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public AgeGroup AgeGroup { get; set; }

        public int OrderIndex { get; set; }
    }

    public class UpdateSectionRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class SectionResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; }
        public int UnitCount { get; set; }
    }

    // ── UNIT ─────────────────────────────────────────────────────────

    public class CreateUnitRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string GuidebookContent { get; set; } = string.Empty;

        [Required]
        public Guid SectionId { get; set; }

        [Required]
        public ContentCategory Category { get; set; }

        public int OrderIndex { get; set; }
        public int XpReward { get; set; } = 100;
    }

    public class UpdateUnitRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string GuidebookContent { get; set; } = string.Empty;
        public ContentCategory Category { get; set; }
        public int OrderIndex { get; set; }
        public int XpReward { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UnitResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GuidebookContent { get; set; } = string.Empty;
        public Guid SectionId { get; set; }
        public string Category { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int XpReward { get; set; }
        public bool IsActive { get; set; }
        public int LessonCount { get; set; }
    }

    // ── LESSON ───────────────────────────────────────────────────────

    public class CreateLessonRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public Guid UnitId { get; set; }

        public int OrderIndex { get; set; }
        public int XpReward { get; set; } = 10;
        public int EstimatedMinutes { get; set; } = 5;
    }

    public class UpdateLessonRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public int OrderIndex { get; set; }
        public int XpReward { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class AdminLessonResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public Guid UnitId { get; set; }
        public int OrderIndex { get; set; }
        public int XpReward { get; set; }
        public int EstimatedMinutes { get; set; }
        public bool IsActive { get; set; }
        public int ExerciseCount { get; set; }
    }

    // ── EXERCISE ─────────────────────────────────────────────────────

    public class CreateExerciseRequest
    {
        [Required]
        public Guid LessonId { get; set; }

        [Required]
        public ExerciseType Type { get; set; }

        public int OrderIndex { get; set; }

        [Required, MaxLength(1000)]
        public string Prompt { get; set; } = string.Empty;

        public string? ExplanationText { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? SentenceTemplate { get; set; }
        public int XpReward { get; set; } = 5;

        /// <summary>
        /// Options for MCQ / TrueFalse / Arrange / Match.
        /// Leave empty for InfoCard type.
        /// </summary>
        public List<CreateExerciseOptionRequest> Options { get; set; } = new();
    }

    public class CreateExerciseOptionRequest
    {
        [Required, MaxLength(500)]
        public string Text { get; set; } = string.Empty;

        public string? TextArabic { get; set; }
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
        public int? MatchGroupId { get; set; }
    }

    public class UpdateExerciseRequest
    {
        public ExerciseType Type { get; set; }
        public int OrderIndex { get; set; }

        [Required, MaxLength(1000)]
        public string Prompt { get; set; } = string.Empty;

        public string? ExplanationText { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? SentenceTemplate { get; set; }
        public int XpReward { get; set; }
        public List<CreateExerciseOptionRequest> Options { get; set; } = new();
    }

    public class ExerciseResponse
    {
        public Guid Id { get; set; }
        public Guid LessonId { get; set; }
        public string Type { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public string Prompt { get; set; } = string.Empty;
        public string? ExplanationText { get; set; }
        public string? AudioUrl { get; set; }
        public string? ImageUrl { get; set; }
        public string? SentenceTemplate { get; set; }
        public int XpReward { get; set; }
        public List<ExerciseOptionResponse> Options { get; set; } = new();
    }

    public class ExerciseOptionResponse
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = string.Empty;
        public string? TextArabic { get; set; }
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
        public int? MatchGroupId { get; set; }
    }

    // ── BRANCH / ACHIEVEMENT (unchanged) ──────────────────────────────

    public class CreateBranchRequest
    {
        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string State { get; set; } = string.Empty;
    }

    public class CreateAchievementRequest
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public string IconUrl { get; set; } = string.Empty;
        public int XpReward { get; set; }

        [Required, MaxLength(50)]
        public string ConditionType { get; set; } = string.Empty;

        public int ConditionValue { get; set; }
    }
}

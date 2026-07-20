namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Records every time a member attempts an exercise.
    /// Used for analytics, mistake review, and practice mode.
    /// </summary>
    public class ExerciseAttempt : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public Guid ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        /// <summary>
        /// The answer the member submitted (option id, text, or ordered ids).
        /// </summary>
        public string AnswerGiven { get; set; } = string.Empty;

        public bool IsCorrect { get; set; }

        /// <summary>
        /// Whether the member used a heart on this attempt (wrong answer).
        /// </summary>
        public bool HeartUsed { get; set; }

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;
    }
}

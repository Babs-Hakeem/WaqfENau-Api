namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Tracks a member's completion status per lesson.
    /// One row per member per lesson.
    /// </summary>
    public class MemberProgress : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;

        public bool IsCompleted { get; set; }
        public DateTime? CompletedAt { get; set; }

        /// <summary>
        /// 0–100: percentage of exercises answered correctly on first attempt.
        /// </summary>
        public int Score { get; set; }

        public int XpEarned { get; set; }

        /// <summary>
        /// How many times this lesson has been replayed (for practice mode).
        /// </summary>
        public int TimesReplayed { get; set; }
    }
}

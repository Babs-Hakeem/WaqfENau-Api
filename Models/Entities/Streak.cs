namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Tracks a member's daily study streak.
    /// A streak is maintained by completing at least one lesson per calendar day.
    /// Streak freezes protect the streak when a day is missed.
    /// </summary>
    public class Streak : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }

        /// <summary>
        /// The last date on which the member completed a lesson (UTC date only).
        /// </summary>
        public DateTime? LastActivityDate { get; set; }

        /// <summary>
        /// Number of streak freezes the member has available.
        /// A freeze is consumed automatically when a day is missed.
        /// Members earn freezes through achievements or XP milestones.
        /// </summary>
        public int FreezesAvailable { get; set; }

        /// <summary>
        /// True if a freeze was used on the last missed day.
        /// </summary>
        public bool FreezeUsedToday { get; set; }
    }
}

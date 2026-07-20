namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Tracks a member's hearts (lives). Max 5 hearts.
    /// One heart is lost per wrong answer.
    /// Hearts refill at a rate of 1 every 30 minutes automatically.
    /// Members can also earn hearts via achievements or daily login bonuses.
    /// </summary>
    public class Hearts : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        /// <summary>Current heart count (0-5).</summary>
        public int Current { get; set; } = 5;

        public const int Max = 5;
        public const int RefillMinutes = 30; // 1 heart refills every 30 min

        /// <summary>
        /// When the next heart will be automatically added.
        /// Null when hearts are full.
        /// </summary>
        public DateTime? NextRefillAt { get; set; }

        public bool IsFull => Current >= Max;
    }
}

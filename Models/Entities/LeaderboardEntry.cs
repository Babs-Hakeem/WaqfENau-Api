namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// One row per member per scope (Branch / National / Friends).
    /// Weekly XP is reset every Monday at midnight UTC.
    /// </summary>
    public class LeaderboardEntry : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        /// <summary>"Branch" | "National" | "Friends"</summary>
        public string Scope { get; set; } = string.Empty;

        public Guid? BranchId { get; set; }

        public int TotalXp { get; set; }

        /// <summary>XP earned this week (resets Monday midnight UTC).</summary>
        public int WeeklyXp { get; set; }

        public int CurrentStreak { get; set; }
        public int LessonsCompleted { get; set; }
        public int Rank { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    }
}

namespace WaqfENau.Api.DTOs
{
    public class MyProfileResponse
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }
        public int DailyGoalMinutes { get; set; }
        public int LessonsCompleted { get; set; }
        public StreakDto Streak { get; set; } = new();
        public HeartsDto Hearts { get; set; } = new();
    }

    public class StreakDto
    {
        public int CurrentStreak { get; set; }
        public int LongestStreak { get; set; }
        public int FreezesAvailable { get; set; }
        public DateTime? LastActivityDate { get; set; }

        /// <summary>
        /// True if the member has already studied today.
        /// Frontend uses this to show the streak as "safe" today.
        /// </summary>
        public bool StudiedToday { get; set; }
    }

    public class HeartsDto
    {
        public int Current { get; set; }
        public int Max { get; set; }
        public DateTime? NextRefillAt { get; set; }
        public bool IsFull { get; set; }
    }

    public class UpdateDailyGoalRequest
    {
        /// <summary>Allowed values: 5, 10, 15, 20</summary>
        public int GoalMinutes { get; set; }
    }

    // ── SECTION PATH (student home screen) ──────────────────────────

    public class SectionPathResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public List<UnitPathResponse> Units { get; set; } = new();
    }

    public class UnitPathResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GuidebookContent { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public int XpReward { get; set; }
        public int TotalLessons { get; set; }
        public int CompletedLessons { get; set; }

        /// <summary>
        /// 0-100 percent of lessons completed in this unit.
        /// </summary>
        public int ProgressPercent { get; set; }

        /// <summary>
        /// True when the previous unit is fully complete (or this is the first unit).
        /// </summary>
        public bool IsUnlocked { get; set; }

        /// <summary>
        /// True when every lesson in this unit is completed.
        /// </summary>
        public bool IsCompleted { get; set; }
    }

    // ── FRIENDS ──────────────────────────────────────────────────────

    public class SendFriendRequestRequest
    {
        public Guid ReceiverId { get; set; }
    }

    public class FriendRequestResponse
    {
        public Guid FriendshipId { get; set; }
        public Guid MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class FriendDto
    {
        public Guid MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int CurrentStreak { get; set; }
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public bool StudiedToday { get; set; }
    }

    public class FriendLeaderboardEntry
    {
        public int Rank { get; set; }
        public Guid MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public int WeeklyXp { get; set; }
        public int CurrentStreak { get; set; }
        public int CurrentLevel { get; set; }
        public bool IsMe { get; set; }
    }
}

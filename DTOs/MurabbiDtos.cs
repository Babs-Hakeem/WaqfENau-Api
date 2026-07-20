namespace WaqfENau.Api.DTOs
{
    public class MemberProgressDto
    {
        public Guid MemberId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string AgeGroup { get; set; } = string.Empty;
        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; }
        public int CurrentStreak { get; set; }
        public int LessonsCompleted { get; set; }
        public double ProgressPercentage { get; set; }
        public DateTime? LastActiveDate { get; set; }
    }

    public class BranchSummaryDto
    {
        public Guid BranchId { get; set; }
        public string BranchName { get; set; } = string.Empty;
        public int TotalMembers { get; set; }
        public int ActiveMembersThisWeek { get; set; }
        public double AverageProgress { get; set; }
        public List<MemberProgressDto> TopPerformers { get; set; } = new();
    }
}

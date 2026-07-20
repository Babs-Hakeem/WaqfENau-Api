namespace WaqfENau.Api.DTOs
{
    public class LeaderboardEntryDto
    {
        public int Rank { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string BranchName { get; set; } = string.Empty;
        public int TotalXp { get; set; }
        public int CurrentStreak { get; set; }
        public int LessonsCompleted { get; set; }
        public int CurrentLevel { get; set; }
    }

    public class LeaderboardRequest
    {
        public string Scope { get; set; } = "Branch"; // "Branch" or "National"
    }

}

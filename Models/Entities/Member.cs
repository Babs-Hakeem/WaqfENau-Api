using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    public class Member : BaseEntity
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;

        public DateTime DateOfBirth { get; set; }
        public AgeGroup AgeGroup { get; set; }
        public UserRole Role { get; set; } = UserRole.Member;

        public Guid BranchId { get; set; }
        public Branch Branch { get; set; } = null!;

        public int TotalXp { get; set; }
        public int CurrentLevel { get; set; } = 1;

        /// <summary>
        /// Member's chosen daily study goal in minutes (5 / 10 / 15 / 20).
        /// </summary>
        public int DailyGoalMinutes { get; set; } = 10;

        public DateTime? LastActiveDate { get; set; }

        // Navigation
        public Hearts? Hearts { get; set; }
        public Streak? Streak { get; set; }
        public ICollection<MemberProgress> Progresses { get; set; } = new List<MemberProgress>();
        public ICollection<ExerciseAttempt> ExerciseAttempts { get; set; } = new List<ExerciseAttempt>();
        public ICollection<XpTransaction> XpTransactions { get; set; } = new List<XpTransaction>();
        public ICollection<MemberAchievement> MemberAchievements { get; set; } = new List<MemberAchievement>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Friendship> SentFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<Friendship> ReceivedFriendRequests { get; set; } = new List<Friendship>();
        public ICollection<LeaderboardEntry> LeaderboardEntries { get; set; } = new List<LeaderboardEntry>();
    }
}

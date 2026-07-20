namespace WaqfENau.Api.Models.Entities
{
    public class Achievement : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconUrl { get; set; } = string.Empty;
        public int XpReward { get; set; }

        /// <summary>
        /// "LessonsCompleted" | "StreakCount" | "TotalXp" | "UnitsCompleted"
        /// </summary>
        public string ConditionType { get; set; } = string.Empty;
        public int ConditionValue { get; set; }

        public ICollection<MemberAchievement> MemberAchievements { get; set; } = new List<MemberAchievement>();
    }
}

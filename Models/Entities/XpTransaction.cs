namespace WaqfENau.Api.Models.Entities
{
    public class XpTransaction : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public int Amount { get; set; }

        /// <summary>
        /// "ExerciseCorrect" | "LessonComplete" | "UnitComplete" |
        /// "StreakBonus" | "Achievement" | "DailyGoal"
        /// </summary>
        public string Reason { get; set; } = string.Empty;
        public Guid? ReferenceId { get; set; }
    }
}

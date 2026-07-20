namespace WaqfENau.Api.Models.Entities
{
    public class MemberAchievement : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public Guid AchievementId { get; set; }
        public Achievement Achievement { get; set; } = null!;

        public DateTime UnlockedAt { get; set; } = DateTime.UtcNow;
    }
}

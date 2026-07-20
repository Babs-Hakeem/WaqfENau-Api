namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// A single node on the learning path map (the star/circle the user taps).
    /// Contains multiple exercises that the user works through one by one.
    /// </summary>
    public class Lesson : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public Guid UnitId { get; set; }
        public Unit Unit { get; set; } = null!;

        public int OrderIndex { get; set; }
        public int XpReward { get; set; } = 10;       // per lesson completion
        public int EstimatedMinutes { get; set; } = 5;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Exercise> Exercises { get; set; } = new List<Exercise>();
        public ICollection<MemberProgress> Progresses { get; set; } = new List<MemberProgress>();
    }
}

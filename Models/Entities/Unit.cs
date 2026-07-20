using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// A unit groups related lessons under a section
    /// (e.g. "Unit 1: Wudu & Salat" under "Section 1").
    /// </summary>
    public class Unit : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string GuidebookContent { get; set; } = string.Empty;

        public Guid SectionId { get; set; }
        public Section Section { get; set; } = null!;

        public ContentCategory Category { get; set; }
        public int OrderIndex { get; set; }
        public int XpReward { get; set; } = 100;

        /// <summary>Draft until explicitly published by the admin.</summary>
        public bool IsPublished { get; set; } = false;
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Top-level grouping on the learning path (e.g. "Section 1").
    /// A section belongs to one age group and contains many units.
    /// Stays unpublished (draft) until the admin explicitly publishes it.
    /// </summary>
    public class Section : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public AgeGroup AgeGroup { get; set; }
        public int OrderIndex { get; set; }

        /// <summary>Draft until explicitly published by the admin.</summary>
        public bool IsPublished { get; set; } = false;

        /// <summary>Soft-disable without deleting (separate from draft state).</summary>
        public bool IsActive { get; set; } = true;

        // Navigation
        public ICollection<Unit> Units { get; set; } = new List<Unit>();
    }
}

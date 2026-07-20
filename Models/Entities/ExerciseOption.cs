namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// One selectable option for an exercise.
    /// Used by MCQ, TrueFalse, Arrange, and Match types.
    ///
    /// For MCQ:      4 options, one IsCorrect = true
    /// For TrueFalse: 2 options ("True" / "False"), one IsCorrect = true
    /// For Arrange:   all options are the words, IsCorrect is ignored —
    ///                correctness is checked by comparing full order
    /// For Match:     MatchGroupId links a left-side item to its right-side pair
    /// </summary>
    public class ExerciseOption : BaseEntity
    {
        public Guid ExerciseId { get; set; }
        public Exercise Exercise { get; set; } = null!;

        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Optional Arabic / Urdu text shown alongside or instead of Text.
        /// </summary>
        public string? TextArabic { get; set; }

        public bool IsCorrect { get; set; }

        /// <summary>
        /// Display order for options. Shuffle on the frontend for MCQ.
        /// </summary>
        public int OrderIndex { get; set; }

        /// <summary>
        /// Used for Match exercises: two options with the same MatchGroupId
        /// are a correct pair (left ↔ right).
        /// </summary>
        public int? MatchGroupId { get; set; }
    }
}

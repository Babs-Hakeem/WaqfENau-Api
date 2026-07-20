using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// One question/card inside a lesson. The user sees these one at a time,
    /// Duolingo-style. Each exercise has a type that tells the frontend
    /// which UI component to render.
    /// </summary>
    public class Exercise : BaseEntity
    {
        public Guid LessonId { get; set; }
        public Lesson Lesson { get; set; } = null!;

        public ExerciseType Type { get; set; }
        public int OrderIndex { get; set; }

        /// <summary>
        /// The question text, instruction, or statement shown to the user.
        /// e.g. "What does 'Alhamdulillah' mean?" or
        ///      "Tap what you hear" or
        ///      "Translate this sentence"
        /// </summary>
        public string Prompt { get; set; } = string.Empty;

        /// <summary>
        /// Optional context shown above the prompt.
        /// e.g. a short explanation for InfoCard type exercises.
        /// </summary>
        public string? ExplanationText { get; set; }

        /// <summary>
        /// Optional URL for audio clip (Quran recitation, Urdu pronunciation).
        /// </summary>
        public string? AudioUrl { get; set; }

        /// <summary>
        /// Optional URL for an image shown with the question.
        /// </summary>
        public string? ImageUrl { get; set; }

        /// <summary>
        /// For FillBlank: the sentence with ___ where the answer goes.
        /// e.g. "Say ___ before eating food."
        /// </summary>
        public string? SentenceTemplate { get; set; }

        /// <summary>
        /// XP awarded for answering this exercise correctly.
        /// </summary>
        public int XpReward { get; set; } = 5;

        // Navigation
        public ICollection<ExerciseOption> Options { get; set; } = new List<ExerciseOption>();
        public ICollection<ExerciseAttempt> Attempts { get; set; } = new List<ExerciseAttempt>();
    }
}

namespace WaqfENau.Api.Models.Enums
{
    public enum ExerciseType
    {
        /// <summary>Read a text/explanation — no answer required, just "continue".</summary>
        InfoCard = 1,

        /// <summary>Pick the correct answer from 4 options.</summary>
        MultipleChoice = 2,

        /// <summary>Pick True or False.</summary>
        TrueFalse = 3,

        /// <summary>Type or select the missing word in a sentence.</summary>
        FillBlank = 4,

        /// <summary>Tap the words in the correct order to form a sentence.</summary>
        Arrange = 5,

        /// <summary>Match left-side items to their right-side pairs.</summary>
        Match = 6,
    }
}

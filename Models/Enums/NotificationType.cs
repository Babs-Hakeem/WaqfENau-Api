namespace WaqfENau.Api.Models.Enums
{
    public enum NotificationType
    {
        StreakReminder = 1,     // "Your streak ends in 2 hours!"
        StreakBroken = 2,       // "Your streak was reset"
        AchievementUnlocked = 3,
        InactivityReminder = 4,
        FriendRequest = 5,
        WeeklyDigest = 6
    }
}

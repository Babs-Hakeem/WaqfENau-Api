namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IGamificationService
    {
        Task<List<string>> CheckAchievementsAsync(Guid memberId);
        Task AwardXpAsync(Guid memberId, int amount, string reason, Guid? referenceId = null);
        Task UpdateLeaderboardAsync(Guid memberId);
    }
}

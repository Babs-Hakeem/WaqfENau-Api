namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IBackgroundWorkerService
    {
        Task CheckInactiveMembersAsync();
        Task ResetBrokenStreaksAsync();
        Task UpdateLeaderboardRanksAsync();
    }
}

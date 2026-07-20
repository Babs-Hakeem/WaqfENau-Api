using WaqfENau.Api.DTOs;

namespace WaqfENau.Api.Infrastructure.Interfaces.Services
{
    public interface IFriendsService
    {
        Task SendFriendRequestAsync(Guid requesterId, Guid receiverId);
        Task AcceptFriendRequestAsync(Guid memberId, Guid friendshipId);
        Task RemoveFriendAsync(Guid memberId, Guid friendshipId);
        Task<List<FriendDto>> GetFriendsAsync(Guid memberId);
        Task<List<FriendRequestResponse>> GetPendingRequestsAsync(Guid memberId);
        Task<List<FriendLeaderboardEntry>> GetFriendLeaderboardAsync(Guid memberId);
    }
}

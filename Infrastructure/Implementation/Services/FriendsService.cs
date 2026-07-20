using WaqfENau.Api.DTOs;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class FriendsService : IFriendsService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FriendsService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task SendFriendRequestAsync(Guid requesterId, Guid receiverId)
        {
            if (requesterId == receiverId)
                throw new Exception("You cannot send a friend request to yourself");

            var receiverExists = await _unitOfWork.Members.GetByIdAsync(receiverId);
            if (receiverExists == null)
                throw new Exception("Member not found");

            var existing = (await _unitOfWork.Repository<Friendship>()
                .FindAsync(f =>
                    (f.RequesterId == requesterId && f.ReceiverId == receiverId) ||
                    (f.RequesterId == receiverId && f.ReceiverId == requesterId)))
                .FirstOrDefault();

            if (existing != null)
                throw new Exception(existing.Status == FriendshipStatus.Accepted
                    ? "You are already friends"
                    : "A friend request already exists between you two");

            var friendship = new Friendship
            {
                RequesterId = requesterId,
                ReceiverId = receiverId,
                Status = FriendshipStatus.Pending
            };

            await _unitOfWork.Repository<Friendship>().AddAsync(friendship);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task AcceptFriendRequestAsync(Guid memberId, Guid friendshipId)
        {
            var friendship = await _unitOfWork.Repository<Friendship>().GetByIdAsync(friendshipId)
                ?? throw new Exception("Friend request not found");

            if (friendship.ReceiverId != memberId)
                throw new Exception("You can only accept requests sent to you");

            if (friendship.Status != FriendshipStatus.Pending)
                throw new Exception("This request is no longer pending");

            friendship.Status = FriendshipStatus.Accepted;
            friendship.AcceptedAt = DateTime.UtcNow;
            friendship.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Repository<Friendship>().Update(friendship);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveFriendAsync(Guid memberId, Guid friendshipId)
        {
            var friendship = await _unitOfWork.Repository<Friendship>().GetByIdAsync(friendshipId)
                ?? throw new Exception("Friendship not found");

            if (friendship.RequesterId != memberId && friendship.ReceiverId != memberId)
                throw new Exception("You are not part of this friendship");

            _unitOfWork.Repository<Friendship>().Delete(friendship);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<List<FriendDto>> GetFriendsAsync(Guid memberId)
        {
            var friendships = await _unitOfWork.Repository<Friendship>()
                .FindAsync(f =>
                    (f.RequesterId == memberId || f.ReceiverId == memberId) &&
                    f.Status == FriendshipStatus.Accepted);

            var result = new List<FriendDto>();
            var today = DateTime.UtcNow.Date;

            foreach (var friendship in friendships)
            {
                var friendId = friendship.RequesterId == memberId
                    ? friendship.ReceiverId
                    : friendship.RequesterId;

                var friend = await _unitOfWork.Members.GetByIdWithDetailsAsync(friendId);
                if (friend == null) continue;

                result.Add(new FriendDto
                {
                    MemberId = friend.Id,
                    FullName = $"{friend.FirstName} {friend.LastName}",
                    CurrentStreak = friend.Streak?.CurrentStreak ?? 0,
                    TotalXp = friend.TotalXp,
                    CurrentLevel = friend.CurrentLevel,
                    BranchName = friend.Branch?.Name ?? string.Empty,
                    StudiedToday = friend.Streak?.LastActivityDate?.Date == today
                });
            }

            return result.OrderByDescending(f => f.CurrentStreak).ToList();
        }

        public async Task<List<FriendRequestResponse>> GetPendingRequestsAsync(Guid memberId)
        {
            var requests = await _unitOfWork.Repository<Friendship>()
                .FindAsync(f => f.ReceiverId == memberId && f.Status == FriendshipStatus.Pending);

            var result = new List<FriendRequestResponse>();

            foreach (var req in requests)
            {
                var requester = await _unitOfWork.Members.GetByIdAsync(req.RequesterId);
                if (requester == null) continue;

                result.Add(new FriendRequestResponse
                {
                    FriendshipId = req.Id,
                    MemberId = requester.Id,
                    FullName = $"{requester.FirstName} {requester.LastName}",
                    Status = req.Status.ToString(),
                    CreatedAt = req.CreatedAt
                });
            }

            return result.OrderByDescending(r => r.CreatedAt).ToList();
        }

        public async Task<List<FriendLeaderboardEntry>> GetFriendLeaderboardAsync(Guid memberId)
        {
            // Get all accepted friendships
            var friendships = await _unitOfWork.Repository<Friendship>()
                .FindAsync(f =>
                    (f.RequesterId == memberId || f.ReceiverId == memberId) &&
                    f.Status == FriendshipStatus.Accepted);

            // Collect friend IDs + include self
            var participantIds = friendships
                .Select(f => f.RequesterId == memberId ? f.ReceiverId : f.RequesterId)
                .ToList();
            participantIds.Add(memberId);

            var entries = new List<FriendLeaderboardEntry>();

            foreach (var id in participantIds)
            {
                var entry = (await _unitOfWork.Leaderboard.GetByMemberAndScopeAsync(id, "National"));
                var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(id);
                if (member == null) continue;

                entries.Add(new FriendLeaderboardEntry
                {
                    MemberId = id,
                    FullName = $"{member.FirstName} {member.LastName}",
                    WeeklyXp = entry?.WeeklyXp ?? 0,
                    CurrentStreak = member.Streak?.CurrentStreak ?? 0,
                    CurrentLevel = member.CurrentLevel,
                    IsMe = id == memberId
                });
            }

            // Rank by weekly XP
            var ranked = entries
                .OrderByDescending(e => e.WeeklyXp)
                .ToList();

            for (int i = 0; i < ranked.Count; i++)
                ranked[i].Rank = i + 1;

            return ranked;
        }
    }
}

using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// Represents a friend relationship between two members.
    /// Friends can see each other's streaks and appear on the
    /// friend leaderboard (weekly reset).
    /// </summary>
    public class Friendship : BaseEntity
    {
        public Guid RequesterId { get; set; }
        public Member Requester { get; set; } = null!;

        public Guid ReceiverId { get; set; }
        public Member Receiver { get; set; } = null!;

        public FriendshipStatus Status { get; set; } = FriendshipStatus.Pending;

        public DateTime? AcceptedAt { get; set; }
    }
}

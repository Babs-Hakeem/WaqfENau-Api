using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Models.Entities
{
    /// <summary>
    /// A notification to be sent to a member.
    /// The DailyWorker schedules streak reminders at the member's preferred time
    /// and sends them via email (or push if integrated later).
    /// </summary>
    public class Notification : BaseEntity
    {
        public Guid MemberId { get; set; }
        public Member Member { get; set; } = null!;

        public NotificationType Type { get; set; }
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public NotificationStatus Status { get; set; } = NotificationStatus.Pending;

        /// <summary>
        /// When this notification should be sent.
        /// For streak reminders this is set to 8pm in the member's local day.
        /// </summary>
        public DateTime? ScheduledAt { get; set; }

        public DateTime? SentAt { get; set; }
        public string? ErrorMessage { get; set; }
    }
}

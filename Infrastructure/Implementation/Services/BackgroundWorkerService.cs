using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using WaqfENau.Api.Infrastructure.Interfaces.Services;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Implementation.Services
{
    public class BackgroundWorkerService : IBackgroundWorkerService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;

        public BackgroundWorkerService(IUnitOfWork unitOfWork, IEmailService emailService)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
        }

        /// <summary>
        /// Sends a streak reminder email to members who haven't studied today
        /// and whose scheduled reminder time has passed (ScheduledAt <= now).
        /// </summary>
        public async Task CheckInactiveMembersAsync()
        {
            var now = DateTime.UtcNow;
            var today = now.Date;

            // Find pending streak reminders scheduled for now or earlier
            var dueReminders = await _unitOfWork.Repository<Notification>()
                .FindAsync(n =>
                    n.Type == NotificationType.StreakReminder &&
                    n.Status == NotificationStatus.Pending &&
                    n.ScheduledAt != null &&
                    n.ScheduledAt <= now);

            foreach (var notification in dueReminders)
            {
                // Check if the member already studied today — if so, skip
                var member = await _unitOfWork.Members.GetByIdWithDetailsAsync(notification.MemberId);
                if (member == null) continue;

                bool studiedToday = member.Streak?.LastActivityDate?.Date == today;
                if (studiedToday)
                {
                    // Cancel this reminder — no longer needed
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = now;
                    _unitOfWork.Repository<Notification>().Update(notification);
                    continue;
                }

                try
                {
                    await _emailService.SendEmailAsync(
                        member.Email,
                        notification.Subject,
                        notification.Body
                    );
                    notification.Status = NotificationStatus.Sent;
                    notification.SentAt = now;
                }
                catch
                {
                    notification.Status = NotificationStatus.Failed;
                }

                _unitOfWork.Repository<Notification>().Update(notification);
            }

            await _unitOfWork.SaveChangesAsync();

            // Schedule tomorrow's reminders for all active members
            await ScheduleTomorrowsRemindersAsync(today.AddDays(1));
        }

        /// <summary>
        /// Checks every member's streak at the end of the day.
        /// If they missed a day:
        ///   - Consume a streak freeze if they have one (streak preserved)
        ///   - Otherwise reset streak to 0 and notify them
        /// </summary>
        public async Task ResetBrokenStreaksAsync()
        {
            var yesterday = DateTime.UtcNow.Date.AddDays(-1);

            // Get all streaks where the last activity was before yesterday
            // (meaning they missed at least one day)
            var brokenStreaks = await _unitOfWork.Streaks.FindAsync(s =>
                s.LastActivityDate.HasValue &&
                s.LastActivityDate.Value.Date < yesterday &&
                s.CurrentStreak > 0);

            foreach (var streak in brokenStreaks)
            {
                if (streak.FreezesAvailable > 0 && !streak.FreezeUsedToday)
                {
                    // ── Consume a freeze — streak is saved ──────────────
                    streak.FreezesAvailable -= 1;
                    streak.FreezeUsedToday = true;
                    // We do NOT reset CurrentStreak
                    _unitOfWork.Streaks.Update(streak);

                    // Notify the member their freeze was used
                    var freezeNotification = new Notification
                    {
                        MemberId = streak.MemberId,
                        Type = NotificationType.StreakReminder,
                        Subject = "Your streak freeze was used!",
                        Body = BuildFreezeUsedEmail(streak),
                        Status = NotificationStatus.Pending,
                        ScheduledAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Notification>().AddAsync(freezeNotification);
                }
                else
                {
                    // ── No freeze available — reset streak ──────────────
                    var previousStreak = streak.CurrentStreak;
                    streak.CurrentStreak = 0;
                    streak.FreezeUsedToday = false;
                    _unitOfWork.Streaks.Update(streak);

                    var brokenNotification = new Notification
                    {
                        MemberId = streak.MemberId,
                        Type = NotificationType.StreakBroken,
                        Subject = "Your streak has been reset 😔",
                        Body = BuildStreakBrokenEmail(previousStreak),
                        Status = NotificationStatus.Pending,
                        ScheduledAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<Notification>().AddAsync(brokenNotification);
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        /// <summary>
        /// Recalculates ranks for branch and national leaderboards.
        /// Weekly XP is reset every Monday.
        /// </summary>
        public async Task UpdateLeaderboardRanksAsync()
        {
            bool isMonday = DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday;

            var branches = await _unitOfWork.Repository<Branch>().GetAllAsync();

            foreach (var branch in branches)
            {
                var entries = (await _unitOfWork.Leaderboard.GetByScopeAsync("Branch", branch.Id, 1000))
                    .OrderByDescending(e => e.TotalXp)
                    .ToList();

                for (int i = 0; i < entries.Count; i++)
                {
                    entries[i].Rank = i + 1;
                    if (isMonday) entries[i].WeeklyXp = 0;
                    _unitOfWork.Leaderboard.Update(entries[i]);
                }
            }

            var nationalEntries = (await _unitOfWork.Leaderboard.GetByScopeAsync("National", null, 1000))
                .OrderByDescending(e => e.TotalXp)
                .ToList();

            for (int i = 0; i < nationalEntries.Count; i++)
            {
                nationalEntries[i].Rank = i + 1;
                if (isMonday) nationalEntries[i].WeeklyXp = 0;
                _unitOfWork.Leaderboard.Update(nationalEntries[i]);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        // ── Private helpers ───────────────────────────────────────────

        private async Task ScheduleTomorrowsRemindersAsync(DateTime tomorrow)
        {
            // For each member that doesn't already have a reminder for tomorrow
            var existingTomorrow = await _unitOfWork.Repository<Notification>()
                .FindAsync(n =>
                    n.Type == NotificationType.StreakReminder &&
                    n.ScheduledAt.HasValue &&
                    n.ScheduledAt.Value.Date == tomorrow);

            var membersWithReminder = existingTomorrow.Select(n => n.MemberId).ToHashSet();

            var activeMembers = await _unitOfWork.Members.FindAsync(m =>
                m.LastActiveDate >= DateTime.UtcNow.AddDays(-30)); // only recently active

            foreach (var member in activeMembers)
            {
                if (membersWithReminder.Contains(member.Id)) continue;

                // Schedule reminder at 8 PM UTC of the target day
                var scheduledAt = tomorrow.AddHours(20);

                var reminder = new Notification
                {
                    MemberId = member.Id,
                    Type = NotificationType.StreakReminder,
                    Subject = "⚠️ Don't break your streak!",
                    Body = BuildStreakReminderEmail(member),
                    Status = NotificationStatus.Pending,
                    ScheduledAt = scheduledAt
                };

                await _unitOfWork.Repository<Notification>().AddAsync(reminder);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        private static string BuildStreakReminderEmail(Member member) => $@"
            <div style='font-family:sans-serif;max-width:500px;margin:auto'>
                <h2>Assalamu Alaikum {member.FirstName}! 🔥</h2>
                <p>Your streak is at risk — you haven't studied today yet.</p>
                <p>Open Waqf-e-Nau and complete at least one lesson before midnight to keep your streak alive!</p>
                <a href='#' style='display:inline-block;padding:12px 24px;background:#4CAF50;color:white;
                   text-decoration:none;border-radius:6px;font-weight:bold;margin-top:12px'>
                   Study Now
                </a>
                <p style='color:#999;font-size:12px;margin-top:24px'>
                   Waqf-e-Nau Nigeria Learning Platform
                </p>
            </div>";

        private static string BuildStreakBrokenEmail(int previousStreak) => $@"
            <div style='font-family:sans-serif;max-width:500px;margin:auto'>
                <h2>Your {previousStreak}-day streak has ended 😔</h2>
                <p>You missed a day of learning. But don't worry — every expert was once a beginner.</p>
                <p>Open Waqf-e-Nau today and start a brand new streak!</p>
                <a href='#' style='display:inline-block;padding:12px 24px;background:#2196F3;color:white;
                   text-decoration:none;border-radius:6px;font-weight:bold;margin-top:12px'>
                   Start Again
                </a>
            </div>";

        private static string BuildFreezeUsedEmail(Streak streak) => $@"
            <div style='font-family:sans-serif;max-width:500px;margin:auto'>
                <h2>Streak Freeze Used! 🛡️</h2>
                <p>You missed yesterday, but your <strong>{streak.CurrentStreak}-day streak</strong> 
                   was protected by a streak freeze.</p>
                <p>You have <strong>{streak.FreezesAvailable}</strong> freeze(s) remaining.</p>
                <p>Make sure to study today to keep your streak going!</p>
                <a href='#' style='display:inline-block;padding:12px 24px;background:#FF9800;color:white;
                   text-decoration:none;border-radius:6px;font-weight:bold;margin-top:12px'>
                   Study Now
                </a>
            </div>";
    }
}

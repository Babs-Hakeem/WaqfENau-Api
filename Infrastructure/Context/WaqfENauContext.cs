using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using WaqfENau.Api.Models.Entities;
using WaqfENau.Api.Models.Enums;

namespace WaqfENau.Api.Infrastructure.Context
{
    public class WaqfENauContext(DbContextOptions<WaqfENauContext> options) : DbContext(options)
    {
        // ── DbSets ────────────────────────────────────────────────────────────

        // Content
        public DbSet<Section> Sections => Set<Section>();
        public DbSet<Unit> Units => Set<Unit>();
        public DbSet<Lesson> Lessons => Set<Lesson>();
        public DbSet<Exercise> Exercises => Set<Exercise>();
        public DbSet<ExerciseOption> ExerciseOptions => Set<ExerciseOption>();
        public DbSet<ExerciseAttempt> ExerciseAttempts => Set<ExerciseAttempt>();

        // Members
        public DbSet<Member> Members => Set<Member>();
        public DbSet<Branch> Branches => Set<Branch>();
        public DbSet<MemberProgress> MemberProgresses => Set<MemberProgress>();

        // Gamification
        public DbSet<Hearts> Hearts => Set<Hearts>();
        public DbSet<Streak> Streaks => Set<Streak>();
        public DbSet<XpTransaction> XpTransactions => Set<XpTransaction>();
        public DbSet<Achievement> Achievements => Set<Achievement>();
        public DbSet<MemberAchievement> MemberAchievements => Set<MemberAchievement>();
        public DbSet<LeaderboardEntry> LeaderboardEntries => Set<LeaderboardEntry>();

        // Social
        public DbSet<Friendship> Friendships => Set<Friendship>();
        public DbSet<Notification> Notifications => Set<Notification>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

        // ── Configuration ─────────────────────────────────────────────────────

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.ConfigureWarnings(w =>
                w.Ignore(RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            ConfigureContent(modelBuilder);
            ConfigureMembers(modelBuilder);
            ConfigureGamification(modelBuilder);
            ConfigureSocial(modelBuilder);

            SeedBranches(modelBuilder);
            SeedAchievements(modelBuilder);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CONTENT
        // ─────────────────────────────────────────────────────────────────────

        private static void ConfigureContent(ModelBuilder m)
        {
            // Section
            m.Entity<Section>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.Title).IsRequired().HasMaxLength(200);
                e.Property(s => s.Description).HasMaxLength(500);
                e.Property(s => s.AgeGroup).HasConversion<string>().HasMaxLength(50);
                e.HasIndex(s => new { s.AgeGroup, s.OrderIndex });

                e.HasMany(s => s.Units)
                    .WithOne(u => u.Section)
                    .HasForeignKey(u => u.SectionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Unit
            m.Entity<Unit>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Title).IsRequired().HasMaxLength(200);
                e.Property(u => u.Description).HasMaxLength(500);
                e.Property(u => u.GuidebookContent).HasColumnType("text");
                e.Property(u => u.Category).HasConversion<string>().HasMaxLength(50);
                e.HasIndex(u => new { u.SectionId, u.OrderIndex });

                e.HasMany(u => u.Lessons)
                    .WithOne(l => l.Unit)
                    .HasForeignKey(l => l.UnitId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Lesson
            m.Entity<Lesson>(e =>
            {
                e.HasKey(l => l.Id);
                e.Property(l => l.Title).IsRequired().HasMaxLength(200);
                e.Property(l => l.Description).HasMaxLength(500);
                e.HasIndex(l => new { l.UnitId, l.OrderIndex });

                e.HasMany(l => l.Exercises)
                    .WithOne(ex => ex.Lesson)
                    .HasForeignKey(ex => ex.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(l => l.Progresses)
                    .WithOne(p => p.Lesson)
                    .HasForeignKey(p => p.LessonId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Exercise
            m.Entity<Exercise>(e =>
            {
                e.HasKey(ex => ex.Id);
                e.Property(ex => ex.Type).HasConversion<string>().HasMaxLength(50);
                e.Property(ex => ex.Prompt).IsRequired().HasMaxLength(1000);
                e.Property(ex => ex.ExplanationText).HasColumnType("text");
                e.Property(ex => ex.AudioUrl).HasMaxLength(500);
                e.Property(ex => ex.ImageUrl).HasMaxLength(500);
                e.Property(ex => ex.SentenceTemplate).HasMaxLength(1000);
                e.HasIndex(ex => new { ex.LessonId, ex.OrderIndex });

                e.HasMany(ex => ex.Options)
                    .WithOne(o => o.Exercise)
                    .HasForeignKey(o => o.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(ex => ex.Attempts)
                    .WithOne(a => a.Exercise)
                    .HasForeignKey(a => a.ExerciseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // ExerciseOption
            m.Entity<ExerciseOption>(e =>
            {
                e.HasKey(o => o.Id);
                e.Property(o => o.Text).IsRequired().HasMaxLength(500);
                e.Property(o => o.TextArabic).HasMaxLength(500);
                e.HasIndex(o => new { o.ExerciseId, o.OrderIndex });
            });

            // ExerciseAttempt
            m.Entity<ExerciseAttempt>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.AnswerGiven).IsRequired().HasMaxLength(1000);
                e.HasIndex(a => new { a.MemberId, a.ExerciseId });
                e.HasIndex(a => a.AttemptedAt);
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // MEMBERS
        // ─────────────────────────────────────────────────────────────────────

        private static void ConfigureMembers(ModelBuilder m)
        {
            // Branch
            m.Entity<Branch>(e =>
            {
                e.HasKey(b => b.Id);
                e.Property(b => b.Name).IsRequired().HasMaxLength(200);
                e.Property(b => b.City).IsRequired().HasMaxLength(100);
                e.Property(b => b.State).IsRequired().HasMaxLength(100);
                e.HasIndex(b => b.Name).IsUnique();
            });

            // Member
            m.Entity<Member>(e =>
            {
                e.HasKey(mb => mb.Id);
                e.Property(mb => mb.FirstName).IsRequired().HasMaxLength(100);
                e.Property(mb => mb.LastName).IsRequired().HasMaxLength(100);
                e.Property(mb => mb.Email).IsRequired().HasMaxLength(255);
                e.HasIndex(mb => mb.Email).IsUnique();
                e.Property(mb => mb.PhoneNumber).HasMaxLength(20);
                e.Property(mb => mb.PasswordHash).IsRequired();
                e.Property(mb => mb.Role).HasConversion<string>().HasMaxLength(50);
                e.Property(mb => mb.AgeGroup).HasConversion<string>().HasMaxLength(50);
                e.Property(mb => mb.DailyGoalMinutes).HasDefaultValue(10);

                e.HasOne(mb => mb.Branch)
                    .WithMany(b => b.Members)
                    .HasForeignKey(mb => mb.BranchId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasOne(mb => mb.Hearts)
                    .WithOne(h => h.Member)
                    .HasForeignKey<Hearts>(h => h.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasOne(mb => mb.Streak)
                    .WithOne(s => s.Member)
                    .HasForeignKey<Streak>(s => s.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.Progresses)
                    .WithOne(p => p.Member)
                    .HasForeignKey(p => p.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.ExerciseAttempts)
                    .WithOne(a => a.Member)
                    .HasForeignKey(a => a.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.XpTransactions)
                    .WithOne(x => x.Member)
                    .HasForeignKey(x => x.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.MemberAchievements)
                    .WithOne(ma => ma.Member)
                    .HasForeignKey(ma => ma.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.Notifications)
                    .WithOne(n => n.Member)
                    .HasForeignKey(n => n.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                e.HasMany(mb => mb.LeaderboardEntries)
                    .WithOne(le => le.Member)
                    .HasForeignKey(le => le.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Friendships — two FK to same table, need explicit names
                e.HasMany(mb => mb.SentFriendRequests)
                    .WithOne(f => f.Requester)
                    .HasForeignKey(f => f.RequesterId)
                    .OnDelete(DeleteBehavior.Restrict);

                e.HasMany(mb => mb.ReceivedFriendRequests)
                    .WithOne(f => f.Receiver)
                    .HasForeignKey(f => f.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MemberProgress
            m.Entity<MemberProgress>(e =>
            {
                e.HasKey(p => p.Id);
                e.HasIndex(p => new { p.MemberId, p.LessonId }).IsUnique();
                e.Property(p => p.Score).HasDefaultValue(0);
                e.Property(p => p.XpEarned).HasDefaultValue(0);
                e.Property(p => p.TimesReplayed).HasDefaultValue(0);
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // GAMIFICATION
        // ─────────────────────────────────────────────────────────────────────

        private static void ConfigureGamification(ModelBuilder m)
        {
            // Hearts
            m.Entity<Hearts>(e =>
            {
                e.HasKey(h => h.Id);
                e.Property(h => h.Current).HasDefaultValue(5);
                e.HasIndex(h => h.MemberId).IsUnique();
                // Ignore computed properties
                e.Ignore(h => h.IsFull);
            });

            // Streak
            m.Entity<Streak>(e =>
            {
                e.HasKey(s => s.Id);
                e.HasIndex(s => s.MemberId).IsUnique();
                e.HasIndex(s => s.LastActivityDate);
                e.Property(s => s.FreezesAvailable).HasDefaultValue(0);
            });

            // XpTransaction
            m.Entity<XpTransaction>(e =>
            {
                e.HasKey(x => x.Id);
                e.Property(x => x.Amount).IsRequired();
                e.Property(x => x.Reason).IsRequired().HasMaxLength(100);
                e.HasIndex(x => new { x.MemberId, x.CreatedAt });
            });

            // Achievement
            m.Entity<Achievement>(e =>
            {
                e.HasKey(a => a.Id);
                e.Property(a => a.Name).IsRequired().HasMaxLength(100);
                e.Property(a => a.Description).HasMaxLength(500);
                e.Property(a => a.ConditionType).IsRequired().HasMaxLength(50);
                e.HasIndex(a => a.Name).IsUnique();

                e.HasMany(a => a.MemberAchievements)
                    .WithOne(ma => ma.Achievement)
                    .HasForeignKey(ma => ma.AchievementId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MemberAchievement
            m.Entity<MemberAchievement>(e =>
            {
                e.HasKey(ma => ma.Id);
                e.HasIndex(ma => new { ma.MemberId, ma.AchievementId }).IsUnique();
            });

            // LeaderboardEntry
            m.Entity<LeaderboardEntry>(e =>
            {
                e.HasKey(le => le.Id);
                e.Property(le => le.Scope).IsRequired().HasMaxLength(20);
                e.HasIndex(le => new { le.MemberId, le.Scope }).IsUnique();
                e.HasIndex(le => new { le.Scope, le.BranchId, le.Rank });
                e.Property(le => le.WeeklyXp).HasDefaultValue(0);
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // SOCIAL
        // ─────────────────────────────────────────────────────────────────────

        private static void ConfigureSocial(ModelBuilder m)
        {
            // Friendship
            m.Entity<Friendship>(e =>
            {
                e.HasKey(f => f.Id);
                e.Property(f => f.Status).HasConversion<string>().HasMaxLength(20);

                // Prevent duplicate friend requests in same direction
                e.HasIndex(f => new { f.RequesterId, f.ReceiverId }).IsUnique();
            });

            // Notification
            m.Entity<Notification>(e =>
            {
                e.HasKey(n => n.Id);
                e.Property(n => n.Type).HasConversion<string>().HasMaxLength(50);
                e.Property(n => n.Status).HasConversion<string>().HasMaxLength(50);
                e.Property(n => n.Subject).IsRequired().HasMaxLength(200);
                e.Property(n => n.Body).HasColumnType("text");
                e.HasIndex(n => new { n.Status, n.ScheduledAt });
            });

            // RefreshToken
            m.Entity<RefreshToken>(e =>
            {
                e.HasKey(rt => rt.Id);
                e.Property(rt => rt.Token).IsRequired().HasMaxLength(500);
                e.HasIndex(rt => rt.Token).IsUnique();
                e.HasIndex(rt => new { rt.MemberId, rt.IsRevoked, rt.ExpiresAt });

                e.HasOne(rt => rt.Member)
                    .WithMany()
                    .HasForeignKey(rt => rt.MemberId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        // ─────────────────────────────────────────────────────────────────────
        // SEED DATA
        // ─────────────────────────────────────────────────────────────────────

        private static void SeedBranches(ModelBuilder m)
        {
            m.Entity<Branch>().HasData(
                new Branch
                {
                    Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    Name = "Agege Jama'at",
                    City = "Lagos",
                    State = "Lagos",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Branch
                {
                    Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    Name = "Apata Jama'at",
                    City = "Ibadan",
                    State = "Oyo",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Branch
                {
                    Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    Name = "Kano Jama'at",
                    City = "Kano",
                    State = "Kano",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Branch
                {
                    Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    Name = "Ife Jama'at",
                    City = "Ife",
                    State = "Osun",
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }

        private static void SeedAchievements(ModelBuilder m)
        {
            m.Entity<Achievement>().HasData(
                new Achievement
                {
                    Id = Guid.Parse("a1111111-1111-1111-1111-111111111111"),
                    Name = "First Steps",
                    Description = "Complete your first lesson",
                    IconUrl = "🏃",
                    XpReward = 50,
                    ConditionType = "LessonsCompleted",
                    ConditionValue = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Achievement
                {
                    Id = Guid.Parse("a2222222-2222-2222-2222-222222222222"),
                    Name = "Week Warrior",
                    Description = "Maintain a 7-day streak",
                    IconUrl = "🔥",
                    XpReward = 100,
                    ConditionType = "StreakCount",
                    ConditionValue = 7,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Achievement
                {
                    Id = Guid.Parse("a3333333-3333-3333-3333-333333333333"),
                    Name = "Seeker of Knowledge",
                    Description = "Earn 1000 XP",
                    IconUrl = "📚",
                    XpReward = 200,
                    ConditionType = "TotalXp",
                    ConditionValue = 1000,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Achievement
                {
                    Id = Guid.Parse("a4444444-4444-4444-4444-444444444444"),
                    Name = "Unit Champion",
                    Description = "Complete your first unit",
                    IconUrl = "🏆",
                    XpReward = 150,
                    ConditionType = "UnitsCompleted",
                    ConditionValue = 1,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Achievement
                {
                    Id = Guid.Parse("a5555555-5555-5555-5555-555555555555"),
                    Name = "Streak Shield",
                    Description = "Reach a 30-day streak",
                    IconUrl = "🛡️",
                    XpReward = 300,
                    ConditionType = "StreakCount",
                    ConditionValue = 30,
                    CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
        }
    }
}

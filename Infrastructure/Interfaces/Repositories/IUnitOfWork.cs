namespace WaqfENau.Api.Infrastructure.Interfaces.Repositories;

public interface IUnitOfWork : IDisposable
{
    IMemberRepository Members { get; }
    ISectionRepository Sections { get; }
    IUnitRepository Units { get; }
    ILessonRepository Lessons { get; }
    IExerciseRepository Exercises { get; }
    IProgressRepository Progresses { get; }
    IStreakRepository Streaks { get; }
    ILeaderboardRepository Leaderboard { get; }
    IBaseRepository<T> Repository<T>() where T : class;

    Task<int> SaveChangesAsync();
}

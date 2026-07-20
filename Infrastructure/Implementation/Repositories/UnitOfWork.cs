using System.Collections;
using WaqfENau.Api.Infrastructure.Context;
using WaqfENau.Api.Infrastructure.Interfaces.Repositories;

namespace WaqfENau.Api.Infrastructure.Implementation.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly WaqfENauContext _context;
    private Hashtable _genericRepositories = new();

    public IMemberRepository Members { get; }
    public ISectionRepository Sections { get; }
    public IUnitRepository Units { get; }
    public ILessonRepository Lessons { get; }
    public IExerciseRepository Exercises { get; }
    public IProgressRepository Progresses { get; }
    public IStreakRepository Streaks { get; }
    public ILeaderboardRepository Leaderboard { get; }

    public UnitOfWork(WaqfENauContext context)
    {
        _context = context;
        Members = new MemberRepository(context);
        Sections = new SectionRepository(context);
        Units = new UnitRepository(context);
        Lessons = new LessonRepository(context);
        Exercises = new ExerciseRepository(context);
        Progresses = new ProgressRepository(context);
        Streaks = new StreakRepository(context);
        Leaderboard = new LeaderboardRepository(context);
    }

    public IBaseRepository<T> Repository<T>() where T : class
    {
        var type = typeof(T).Name;

        if (!_genericRepositories.ContainsKey(type))
        {
            var repositoryType = typeof(BaseRepository<>);
            var repositoryInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(T)), _context);
            _genericRepositories.Add(type, repositoryInstance);
        }

        return (IBaseRepository<T>)_genericRepositories[type]!;
    }

    public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}

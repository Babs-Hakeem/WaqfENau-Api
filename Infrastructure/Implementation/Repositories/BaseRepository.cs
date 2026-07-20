using WaqfENau.Api.Infrastructure.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using WaqfENau.Api.Infrastructure.Context;
namespace WaqfENau.Api.Infrastructure.Implementation.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        protected readonly WaqfENauContext _context;
        protected readonly DbSet<T> _dbSet;

        public BaseRepository(WaqfENauContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public async Task<T?> GetByIdAsync(Guid id) => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> GetAllAsync() => await _dbSet.ToListAsync();

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> expression)
            => await _dbSet.Where(expression).ToListAsync();

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> expression)
            => await _dbSet.FirstOrDefaultAsync(expression);

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> expression)
            => await _dbSet.AnyAsync(expression);

        public async Task<int> CountAsync(Expression<Func<T, bool>>? expression = null)
            => expression == null ? await _dbSet.CountAsync() : await _dbSet.CountAsync(expression);

        public async Task AddAsync(T entity) => await _dbSet.AddAsync(entity);

        public async Task AddRangeAsync(IEnumerable<T> entities) => await _dbSet.AddRangeAsync(entities);

        public void Update(T entity) => _dbSet.Update(entity);

        public void Delete(T entity) => _dbSet.Remove(entity);

        public void DeleteRange(IEnumerable<T> entities) => _dbSet.RemoveRange(entities);
    }
}

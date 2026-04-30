using EsperantOS.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace EsperantOS.DataAccess.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly EsperantOSContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(EsperantOSContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public virtual async Task<T?> GetByIdAsync(int id)
        {
            return await _dbSet.FindAsync(id);
        }

        public virtual async Task AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public virtual async Task UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            await Task.CompletedTask;
        }

        public virtual async Task DeleteAsync(int id)
        {
            var entity = await GetByIdAsync(id);
            if (entity != null)
                _dbSet.Remove(entity);
        }
    }
}

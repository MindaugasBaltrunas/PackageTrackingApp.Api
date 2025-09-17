using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Data.Context;
using PackageTrackingApp.Domain.Interfaces;

namespace PackageTrackingApp.Data.Repositories
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public BaseRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _dbSet = _context.Set<T>();
        }

       public virtual async Task<T> AddAsync(T entity)
        {    
            await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
            return entity;
        }

        public virtual async Task<T?> GetByIdAsync(Guid id)
        {
            return await _dbSet.FindAsync(id);
        }
    }
}

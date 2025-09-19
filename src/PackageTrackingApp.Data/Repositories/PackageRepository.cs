using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Data.Context;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Domain.Interfaces;

namespace PackageTrackingApp.Data.Repositories
{
    public class PackageRepository : IPackageRepository
    {
        private readonly AppDbContext _context;

        public PackageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Package> AddAsync(Package package)
        {
            _context.Packages.Add(package);
            await _context.SaveChangesAsync();

            return package;
        }

        public async Task<List<Package>> GetAllAsync()
        {
            return await _context.Packages
                .Include(s => s.Sender)
                .Include(r => r.Recipient).ToListAsync();
        }

        public async Task<Package?> GetAsync(Guid id)
        {
            return await _context.Packages
                .Include(p => p.StatusHistory)
                .Include(s => s.Sender)
                .Include(r => r.Recipient)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<List<Package>> FilterAllAsync(string? trackingNumber, PackageStatus? status)
        {
            var query = _context.Packages
                .Include(s => s.Sender)
                .Include(r => r.Recipient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(trackingNumber))
            {
                query = query.Where(p => p.TrackingNumber == trackingNumber);
            }

            if (status.HasValue)
            {

                query = query.Where(p => p.CurrentStatus == status.Value);

            }
            return await query.ToListAsync();
        }

        public async Task<Package?> UpdateAsync(PackageStatus status, Package package)
        {
            package.CurrentStatus = status;

            _context.Attach(package);
            _context.Entry(package).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return package;
        }

    }
}


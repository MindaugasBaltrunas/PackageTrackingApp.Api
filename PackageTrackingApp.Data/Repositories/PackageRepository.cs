
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
            return await _context.Packages.ToListAsync(); ;
        }

        public async Task<List<Package>> FilterAllAsync(int? trackingNumber, string? status)
        {
            var query = _context.Packages.AsQueryable();

            if (trackingNumber.HasValue)
            {
                query = query.Where(p => p.TrackingNumber == trackingNumber.Value.ToString());
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalizedStatus = status.Trim();
                if (!Enum.TryParse<PackageStatus>(normalizedStatus, ignoreCase: true, out var parsedStatus))
                    return new List<Package>();

                query = query.Where(p => p.CurrentStatus == parsedStatus);

            }
            return await query.ToListAsync();
        }

        public async Task<Package?> ExchangeAsync(PackageStatus status, Guid id)
        {
            var package = await _context.Packages
             .Include(p => p.StatusHistory)
             .FirstOrDefaultAsync(x => x.Id == id);

            if (package == null)
                return null;

            if (package.CurrentStatus == status)
                return package;

            package.StatusHistory.Add(new PackageStatusHistory
            {
                Id = Guid.NewGuid(),
                PackageId = package.Id,
                Status = package.CurrentStatus,
                ChangedAt = DateTime.UtcNow
            });

            package.CurrentStatus = status;

            await _context.SaveChangesAsync();

            return package;
        }



    }
}



using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Data.Context;
using PackageTrackingApp.Domain.Entities;
using PackageTrackingApp.Domain.Interfaces;
using System;

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

        public async Task<Package?> GetAsync(Guid id)
        {

            return await _context.Packages.Where(x => x.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Package>> FilterAllAsync(string? trackingNumber, PackageStatus? status)
        {
            var query = _context.Packages.AsQueryable();

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

        public async Task<Package?> ExchangeAsync(PackageStatus status, Package package )
        {      

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


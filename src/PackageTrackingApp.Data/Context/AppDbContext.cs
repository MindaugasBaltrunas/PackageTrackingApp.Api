using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Domain.Entities;

namespace PackageTrackingApp.Data.Context
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Package> Packages { get; set; }
        public DbSet<Sender> Senders { get; set; }
        public DbSet<PackageStatusHistory> PackageStatusHistory { get; set; }
        public DbSet<Recipient> Recipients { get; set; }



        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
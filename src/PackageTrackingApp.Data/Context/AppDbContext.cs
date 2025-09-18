using Microsoft.EntityFrameworkCore;
using PackageTrackingApp.Domain.Entities;
using System.Reflection.Emit;

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
            builder.Entity<Package>()
             .HasOne(p => p.Sender)
             .WithOne(s => s.Package)
             .HasForeignKey<Package>(p => p.SenderId);

            builder.Entity<Package>()
             .HasOne(p => p.Recipient)
             .WithOne(s => s.Package)
             .HasForeignKey<Package>(p => p.RecipientId);

            builder.Entity<Package>()
             .HasMany(p => p.StatusHistory)
             .WithOne(s => s.Package)
             .HasForeignKey(s => s.PackageId)
             .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
        }
    }
}
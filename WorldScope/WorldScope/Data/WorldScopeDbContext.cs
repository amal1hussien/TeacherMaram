using Microsoft.EntityFrameworkCore;
using WorldScope.API.Models;

namespace WorldScope.API.Data
{
    public class WorldScopeDbContext : DbContext
    {
        public WorldScopeDbContext(
            DbContextOptions<WorldScopeDbContext> options)
            : base(options)
        {
        }

        public DbSet<Country> Countries { get; set; }

        public DbSet<Favorite> Favorites { get; set; }

        public DbSet<Comparison> Comparisons { get; set; }

        public DbSet<ComparisonCountry> ComparisonCountries
        { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>()
                .Property(c => c.Area)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Favorite>()
                .HasIndex(f => f.CountryCode)
                .IsUnique();

            modelBuilder.Entity<Favorite>()
                .HasOne(f => f.Country)
                .WithMany(c => c.Favorites)
                .HasForeignKey(f => f.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ComparisonCountry>()
                .HasKey(cc => new
                {
                    cc.ComparisonId,
                    cc.CountryCode
                });

            modelBuilder.Entity<ComparisonCountry>()
                .HasOne(cc => cc.Comparison)
                .WithMany(c => c.ComparisonCountries)
                .HasForeignKey(cc => cc.ComparisonId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ComparisonCountry>()
                .HasOne(cc => cc.Country)
                .WithMany(c => c.ComparisonCountries)
                .HasForeignKey(cc => cc.CountryCode)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
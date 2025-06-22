using Microsoft.EntityFrameworkCore;
using SquaresApi.Models;

namespace SquaresApi.Data
{
    public class PointsContext : DbContext  // Entity Framework DbContext for storing and accessing 2D point data
    {
        public PointsContext(DbContextOptions<PointsContext> options) : base(options) { }

        public DbSet<Point> Points { get; set; }

        // Configure entity properties and constraints (e.g., primary key)
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Point>()
                .HasKey(p => p.PointId);

            modelBuilder.Entity<Point>()
                .HasIndex(p => new { p.CoordinateX, p.CoordinateY })
                .IsUnique();
        }
    }
}

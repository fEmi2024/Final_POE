using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Final_POE.Models;

namespace Final_POE.Data
{
    public class ApplicationDbContext: DbContext
    {
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // The Claims table
        public DbSet<Claims> Claims { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configure the enum Status to be stored as a string
            modelBuilder.Entity<Claims>()
                .Property(c => c.Status)
                .HasConversion<string>();
        }
    }
}


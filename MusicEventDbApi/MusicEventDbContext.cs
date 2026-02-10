using Microsoft.EntityFrameworkCore;

namespace MusicEventDbApi
{
    public class MusicEventDbContext : DbContext
    {
        public DbSet<MusicEvent> Events { get; set; }

        public MusicEventDbContext(DbContextOptions<MusicEventDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<MusicEvent>(entity =>
            {
                entity.ToTable("events");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id).ValueGeneratedOnAdd();
            });
        }
    }
}

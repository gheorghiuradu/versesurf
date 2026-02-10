using Microsoft.EntityFrameworkCore;
using MusicDbApi.Models;

namespace MusicDbApi
{
    public class MusicDbContext : DbContext
    {
        public DbSet<Playlist> Playlists { get; set; }
        public DbSet<Song> Songs { get; set; }

        public MusicDbContext(DbContextOptions<MusicDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Playlist>(entity =>
            {
                entity.ToTable("playlists");
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Enabled).HasDefaultValue(true);
                entity.Property(e => e.Featured).HasDefaultValue(false);
                entity.Property(e => e.Votes).HasDefaultValue(0);
                entity.Property(e => e.Plays).HasDefaultValue(0);
                entity.HasMany(e => e.Songs)
                    .WithOne()
                    .HasForeignKey(s => s.PlaylistId);
            });

            modelBuilder.Entity<Song>(entity =>
            {
                entity.ToTable("songs");
                entity.HasKey(e => e.Id);
            });
        }
    }
}

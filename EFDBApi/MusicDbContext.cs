using Microsoft.EntityFrameworkCore;
using SharedDomain.Database;

namespace EFDBApi
{
    public class MusicDbContext : DbContext
    {
        private readonly string connectionString;

        public virtual DbSet<DbSong> DbSongs { get; set; }
        public virtual DbSet<DbPlaylist> DbPlaylists { get; set; }
        public virtual DbSet<DbPlaylistsSongs> DbPlaylistsSongs { get; set; }

        public MusicDbContext(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public MusicDbContext()
        {
            this.connectionString =
                "Host=localhost;Port=5432;Database=music-db;Username=developer;Password=kt7Hdzkk";
        }

        public MusicDbContext(DbContextOptions<MusicDbContext> opt)
            : base(opt)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql(this.connectionString);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<DbPlaylistsSongs>(entity =>
            {
                entity.ToTable("playlists_songs");

                entity.HasKey(e => e.Id);
                entity.HasOne(e => e.DbPlaylist)
                .WithMany(e => e.DbPlaylistsSongs)
                .HasForeignKey(e => e.PlaylistId);

                entity.HasOne(e => e.DbSong)
                .WithMany(e => e.DbPlaylistsSongs)
                .HasForeignKey(e => e.SongId);

                entity.Property(e => e.Enabled).IsRequired().HasDefaultValue(true);
            });

            modelBuilder.Entity<DbSong>(entity =>
            {
                entity.ToTable("songs");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SpotifyId)
                    .IsUnique();
                entity.Property(e => e.SpotifyId)
                .IsRequired();
                entity.Property(e => e.Artist)
                .IsRequired();
                entity.Property(e => e.Title).IsRequired();
                entity.Property(e => e.Snippet).IsRequired();
                entity.Property(e => e.PreviewUrl).IsRequired();
                entity.Property(e => e.BMIWorkNumber).IsRequired();
                entity.Property(e => e.Plays).HasDefaultValue(0);
            });

            modelBuilder.Entity<DbPlaylist>(entity =>
            {
                entity.ToTable("playlists");

                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.SpotifyId)
                .IsUnique();
                entity.Property(e => e.SpotifyId).IsRequired();
                entity.Property(e => e.Name).IsRequired();
                entity.Property(e => e.Enabled).IsRequired().HasDefaultValue(true);
                entity.Property(e => e.Featured).IsRequired().HasDefaultValue(false);
                entity.Property(e => e.Votes).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Plays).IsRequired().HasDefaultValue(0);
                entity.Property(e => e.Free).IsRequired().HasDefaultValue(false);
            });
        }
    }
}
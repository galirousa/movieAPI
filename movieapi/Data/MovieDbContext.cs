using Microsoft.EntityFrameworkCore;
using movieapi;

namespace movieapi.Data
{
    public class MovieDbContext : DbContext
    {
        public MovieDbContext(DbContextOptions<MovieDbContext> options) : base(options)
        {
        }

        public DbSet<Movie> Movies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Movie>(entity =>
            {
                entity.ToTable("movies");

                // Clave primaria usando tmdb_id
                entity.HasKey(e => e.TmdbId);
                entity.Property(e => e.TmdbId).HasColumnName("tmdb_id").IsRequired();
                
                // Propiedades de la película
                entity.Property(e => e.Title).HasColumnName("title").IsRequired().HasMaxLength(500);
                entity.Property(e => e.OriginalTitle).HasColumnName("original_title").HasMaxLength(500);
                entity.Property(e => e.Overview).HasColumnName("overview").HasColumnType("TEXT");
                entity.Property(e => e.ReleaseDateString).HasColumnName("release_date").HasMaxLength(50);
                entity.Property(e => e.PosterPath).HasColumnName("poster_path").HasMaxLength(200);
                entity.Property(e => e.BackdropPath).HasColumnName("backdrop_path").HasMaxLength(200);
                entity.Property(e => e.Adult).HasColumnName("adult").HasDefaultValue(false);
                entity.Property(e => e.GenreIds).HasColumnName("genre_ids").HasColumnType("integer[]");
                entity.Property(e => e.OriginalLanguage).HasColumnName("original_language").HasMaxLength(10);
                entity.Property(e => e.Popularity).HasColumnName("popularity").HasColumnType("decimal(10,3)");
                entity.Property(e => e.VoteAverage).HasColumnName("vote_average").HasColumnType("decimal(3,1)");
                entity.Property(e => e.VoteCount).HasColumnName("vote_count");
                entity.Property(e => e.Video).HasColumnName("video").HasDefaultValue(false);
                entity.Property(e => e.LastUpdated).HasColumnName("last_updated").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
                entity.Property(e => e.CreatedAt).HasColumnName("created_at").HasColumnType("timestamp with time zone").HasDefaultValueSql("CURRENT_TIMESTAMP");
            });
        }
    }
}
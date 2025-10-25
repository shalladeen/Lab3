using Microsoft.EntityFrameworkCore;
using PodcastApp.Models;

namespace PodcastApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Podcast> Podcasts => Set<Podcast>();
        public DbSet<Episode> Episodes => Set<Episode>();
        public DbSet<Subscription> Subscriptions => Set<Subscription>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // map exact table names
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Podcast>().ToTable("Podcasts");
            modelBuilder.Entity<Episode>().ToTable("Episodes");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");

            // relationships: Podcast (1) -> Episodes (many)
            modelBuilder.Entity<Podcast>()
                .HasMany(p => p.Episodes)
                .WithOne(e => e.Podcast!)
                .HasForeignKey(e => e.PodcastID);

            // ensure string keys are non-unicode/varchar length 10 (matches your SQL)
            modelBuilder.Entity<Episode>().Property(e => e.EpisodeID).IsUnicode(false).HasMaxLength(10);
            modelBuilder.Entity<Episode>().Property(e => e.PodcastID).IsUnicode(false).HasMaxLength(10);
            modelBuilder.Entity<Podcast>().Property(p => p.PodcastID).IsUnicode(false).HasMaxLength(10);
        }
    }
}

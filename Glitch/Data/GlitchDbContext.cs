using Glitch.Classes;
using Microsoft.EntityFrameworkCore;

namespace Glitch.Data;

public sealed class GlitchDbContext(DbContextOptions<GlitchDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Swipe> Swipes => Set<Swipe>();

    public DbSet<Match> Matches => Set<Match>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.UserID);

            entity.HasIndex(user => user.Email)
                .IsUnique();

            entity.Property(user => user.Email)
                .IsRequired()
                .HasMaxLength(320);

            entity.Property(user => user.Password)
                .IsRequired();

            entity.HasOne(user => user.Profile)
                .WithOne()
                .HasForeignKey<Profile>(profile => profile.UserID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Profile>(entity =>
        {
            entity.HasKey(profile => profile.ProfileID);

            entity.Property(profile => profile.DisplayName)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Swipe>(entity =>
        {
            entity.HasKey(swipe => swipe.ID);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(swipe => swipe.FromPrifileID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(swipe => swipe.ToProfileID)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Match>(entity =>
        {
            entity.HasKey(match => match.ID);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(match => match.ProfileAID)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(match => match.ProfileBID)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
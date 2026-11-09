using Glitch.Classes;
using Microsoft.EntityFrameworkCore;

namespace Glitch.Data;

public sealed class GlitchDbContext(DbContextOptions<GlitchDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Profile> Profiles => Set<Profile>();

    public DbSet<Swipe> Swipes => Set<Swipe>();
    
    public DbSet<Match> Matches => Set<Match>();
    
    public DbSet<Message> Messages => Set<Message>();

    public DbSet<Condition> Conditions => Set<Condition>();

    public DbSet<Interest> Interests => Set<Interest>();

    public DbSet<ProfileCondition> ProfileConditions => Set<ProfileCondition>();

    public DbSet<ProfileInterest> ProfileInterests => Set<ProfileInterest>();

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

            entity.Property(profile => profile.Bio)
                .HasMaxLength(2000);

            entity.Property(profile => profile.ImagePath)
                .HasMaxLength(500);
        });

        modelBuilder.Entity<Swipe>(entity =>
        {
            entity.HasKey(swipe => swipe.ID);

            entity.HasIndex(swipe => new
            {
                swipe.FromProfileID,
                swipe.ToProfileID
            })
            .IsUnique();

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(swipe => swipe.FromProfileID)
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
        
        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(msg => msg.ID);

            entity.Property(msg => msg.Text)
                .IsRequired()
                .HasMaxLength(4000);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(msg => msg.SenderProfileID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Condition>(entity =>
        {
            entity.HasKey(condition => condition.ID);

            entity.Property(condition => condition.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(condition => condition.Category)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<Interest>(entity =>
        {
            entity.HasKey(interest => interest.InterestID);

            entity.Property(interest => interest.Name)
                .IsRequired()
                .HasMaxLength(100);
        });

        modelBuilder.Entity<ProfileCondition>(entity =>
        {
            entity.HasKey(profileCondition => new
            {
                profileCondition.Profilefield,
                profileCondition.ConditionID
            });

            entity.HasIndex(profileCondition => profileCondition.ConditionID);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(profileCondition => profileCondition.Profilefield)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Condition>()
                .WithMany()
                .HasForeignKey(profileCondition => profileCondition.ConditionID)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProfileInterest>(entity =>
        {
            entity.HasKey(profileInterest => new
            {
                profileInterest.ProfileID,
                profileInterest.InterestID
            });

            entity.HasIndex(profileInterest => profileInterest.InterestID);

            entity.HasOne<Profile>()
                .WithMany()
                .HasForeignKey(profileInterest => profileInterest.ProfileID)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<Interest>()
                .WithMany()
                .HasForeignKey(profileInterest => profileInterest.InterestID)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

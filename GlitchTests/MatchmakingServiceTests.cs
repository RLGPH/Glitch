using Glitch.Classes;
using Glitch.Methods;

namespace GlitchTests;

public sealed class MatchmakingServiceTests
{
    [Fact]
    public async Task Returns_Profiles_Ordered_By_Last_Updated()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User",
                LastUpdatedAt = new DateTime(2026, 1, 1)
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Older Profile",
                LastUpdatedAt = new DateTime(2026, 1, 2)
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Newer Profile",
                LastUpdatedAt = new DateTime(2026, 1, 3)
            });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [],
            []);

        Assert.Equal(2, results.Count);
        Assert.Equal(3, results[0].ProfileID);
        Assert.Equal(2, results[1].ProfileID);
    }

    [Fact]
    public async Task Excludes_Current_Profile_And_ActiveMatches()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User"
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Matched User"
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Available User"
            });

        context.Matches.Add(new Match
        {
            ID = 1,
            ProfileAID = 1,
            ProfileBID = 2
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [],
            []);

        var profile = Assert.Single(results);

        Assert.Equal(3, profile.ProfileID);
    }

    [Fact]
    public async Task Excludes_Previously_Swiped_Profiles()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User"
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Previously Swiped User"
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Available User"
            });

        context.Swipes.Add(new Swipe
        {
            ID = 1,
            FromProfileID = 1,
            ToProfileID = 2,
            IsLiked = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [],
            []);

        var profile = Assert.Single(results);

        Assert.Equal(3, profile.ProfileID);
    }

    [Fact]
    public async Task Applies_Condition_Filters()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User"
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Matching User"
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Non-Matching User"
            });

        context.Conditions.Add(new Condition
        {
            ID = 100,
            Name = "Condition",
            Category = "Test"
        });

        context.ProfileConditions.Add(new ProfileCondition
        {
            Profilefield = 2,
            ConditionID = 100
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [100],
            []);

        var profile = Assert.Single(results);

        Assert.Equal(2, profile.ProfileID);
    }

    [Fact]
    public async Task Applies_Interest_Filters()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User"
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Matching User"
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Non-Matching User"
            });

        context.Interests.Add(new Interest
        {
            InterestID = 200,
            Name = "Music"
        });

        context.ProfileInterests.Add(new ProfileInterest
        {
            ProfileID = 2,
            InterestID = 200
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [],
            [200]);

        var profile = Assert.Single(results);

        Assert.Equal(2, profile.ProfileID);
    }

    [Fact]
    public async Task Reset_Round_Includes_Historical_Swipes()
    {
        await using var context = CreateContext();

        context.Profiles.AddRange(
            new Profile
            {
                ProfileID = 1,
                DisplayName = "Current User"
            },
            new Profile
            {
                ProfileID = 2,
                DisplayName = "Previously Swiped User"
            });

        context.Swipes.Add(new Swipe
        {
            ID = 1,
            FromProfileID = 1,
            ToProfileID = 2,
            IsLiked = true,
            CreatedAt = DateTime.UtcNow
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(
            1,
            [],
            [],
            true);

        var profile = Assert.Single(results);

        Assert.Equal(2, profile.ProfileID);
    }

    private static Glitch.Data.GlitchDbContext CreateContext()
    {
        return new TestDbContextFactory().CreateDbContext();
    }
}
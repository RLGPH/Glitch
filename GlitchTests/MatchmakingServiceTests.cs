using Glitch.Classes;
using Glitch.Methods;

namespace GlitchTests;

public sealed class MatchmakingServiceTests
{
    [Fact]
    public async Task GetPotentialMatchesAsync_CalculatesSharedScores()
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
                DisplayName = "Matching User",
                LastUpdatedAt = new DateTime(2026, 1, 2)
            });

        context.ProfileConditions.AddRange(
            new ProfileCondition
            {
                Profilefield = 1,
                ConditionID = 100
            },
            new ProfileCondition
            {
                Profilefield = 2,
                ConditionID = 100
            });

        context.ProfileInterests.AddRange(
            new ProfileInterest
            {
                ProfileID = 1,
                InterestID = 200
            },
            new ProfileInterest
            {
                ProfileID = 2,
                InterestID = 200
            });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(1);

        var match = Assert.Single(results);

        Assert.Equal(2, match.Profile.ProfileID);
        Assert.Equal(1, match.SharedConditionsCount);
        Assert.Equal(1, match.SharedInterestsCount);
        Assert.Equal(15, match.MatchScore);
    }

    [Fact]
    public async Task GetPotentialMatchesAsync_ExcludesCurrentProfileAndPreviouslySwipedProfiles()
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
            IsLiked = false
        });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(1);

        var match = Assert.Single(results);

        Assert.Equal(3, match.Profile.ProfileID);
        Assert.DoesNotContain(results, result => result.Profile.ProfileID == 1);
        Assert.DoesNotContain(results, result => result.Profile.ProfileID == 2);
    }

    [Fact]
    public async Task GetPotentialMatchesAsync_OrdersResultsByMatchScoreDescending()
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
                DisplayName = "Lower Match"
            },
            new Profile
            {
                ProfileID = 3,
                DisplayName = "Higher Match"
            });

        context.ProfileConditions.AddRange(
            new ProfileCondition
            {
                Profilefield = 1,
                ConditionID = 100
            },
            new ProfileCondition
            {
                Profilefield = 2,
                ConditionID = 100
            },
            new ProfileCondition
            {
                Profilefield = 3,
                ConditionID = 100
            },
            new ProfileCondition
            {
                Profilefield = 3,
                ConditionID = 101
            });

        await context.SaveChangesAsync();

        var service = new Matchmaking_service(context);

        var results = await service.GetPotentialMatchesAsync(1);

        Assert.Equal(2, results.Count);
        Assert.Equal(3, results[0].Profile.ProfileID);
        Assert.Equal(2, results[1].Profile.ProfileID);
        Assert.True(results[0].MatchScore > results[1].MatchScore);
    }

    private static Glitch.Data.GlitchDbContext CreateContext()
    {
        return new TestDbContextFactory().CreateDbContext();
    }
}
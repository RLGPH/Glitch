using Glitch.Classes;
using Glitch.Methods;

namespace GlitchTests;

public sealed class LoginServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ReturnsFailure_WhenEmailOrPasswordIsMissing()
    {
        var service = new LoginService(new TestDbContextFactory());

        var result = await service.AuthenticateAsync(string.Empty, string.Empty);

        Assert.False(result.Success);
        Assert.Equal("Email and password are required.", result.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsFailure_WhenPasswordIsIncorrect()
    {
        var factory = new TestDbContextFactory();

        await using (var context = factory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                UserID = 1,
                Email = "user@example.com",
                Password = "correct-password",
                IsActive = true
            });

            await context.SaveChangesAsync();
        }

        var service = new LoginService(factory);

        var result = await service.AuthenticateAsync(
            "user@example.com",
            "incorrect-password");

        Assert.False(result.Success);
        Assert.Equal("Invalid email or password.", result.Message);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsSuccess_WithTheUserProfile()
    {
        var factory = new TestDbContextFactory();

        await using (var context = factory.CreateDbContext())
        {
            context.Users.Add(new User
            {
                UserID = 1,
                Email = "user@example.com",
                Password = "correct-password",
                IsActive = true,
                Profile = new Profile
                {
                    ProfileID = 1,
                    UserID = 1,
                    DisplayName = "Test User"
                }
            });

            await context.SaveChangesAsync();
        }

        var service = new LoginService(factory);

        var result = await service.AuthenticateAsync(
            "user@example.com",
            "correct-password");

        Assert.True(result.Success);
        Assert.Equal("Login successful.", result.Message);
        Assert.NotNull(result.UserProfile);
        Assert.Equal("Test User", result.UserProfile.DisplayName);
    }
}
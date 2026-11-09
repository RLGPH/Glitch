using Glitch.Classes;
using Glitch.Data;
using Microsoft.EntityFrameworkCore;

public sealed class LoginService(IDbContextFactory<GlitchDbContext> contextFactory)
{
    public async Task<LoginResult> AuthenticateAsync(
        string email,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return new LoginResult
            {
                Success = false,
                Message = "Email and password are required."
            };
        }

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var user = await context.Users
            .Include(currentUser => currentUser.Profile)
            .SingleOrDefaultAsync(
                currentUser => currentUser.Email == email,
                cancellationToken);

        if (user is null || !user.IsActive)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        if (user.Password != password)
        {
            return new LoginResult
            {
                Success = false,
                Message = "Invalid email or password."
            };
        }

        return new LoginResult
        {
            Success = true,
            Message = "Login successful.",
            UserProfile = user.Profile
        };
    }
}
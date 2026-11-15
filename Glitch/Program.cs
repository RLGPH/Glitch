using Glitch.Components;
using Glitch.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Glitch.Hubs;
using Glitch.Methods;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Blazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// SQLite - eksisterende database
builder.Services.AddDbContextFactory<GlitchDbContext>(options =>
    options.UseSqlite(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Eksisterende loginservice
builder.Services.AddScoped<LoginService>();

// Cookie-baseret login uden ASP.NET Identity
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Glitch.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;

        options.LoginPath = "/login";
        options.AccessDeniedPath = "/login";

        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
    });

// Autorisation og loginstatus i Blazor
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddSignalR();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Statiske filer, herunder uploadede profilbilleder
app.UseStaticFiles();

// Cookie-login og adgangskontrol
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

// Statiske assets fra Blazor
app.MapStaticAssets();

// Blazor-komponenter
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// SignalR endpoint til chat
app.MapHub<ChatHub>("/chathub")
    .RequireAuthorization();

// Logout
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    return Results.LocalRedirect("/login");
})
.RequireAuthorization();


// =========================================
// DELETE USER ACCOUNT
// =========================================

// Delete the signed-in account and its related data
app.MapPost("/auth/delete-account", async (
    HttpContext context,
    IDbContextFactory<GlitchDbContext> dbFactory,
    IWebHostEnvironment environment) =>
{
    // Get the authenticated user's ID
    if (!int.TryParse(
        context.User.FindFirstValue(ClaimTypes.NameIdentifier),
        out int userId))
    {
        return Results.Forbid();
    }

    await using var db =
        await dbFactory.CreateDbContextAsync();

    // Find the user's uploaded profile image
    string? imagePath = await db.Profiles
        .Where(p => p.UserID == userId)
        .Select(p => p.ImagePath)
        .FirstOrDefaultAsync();

    // Delete the user account and related database records
    bool deleted = await new AccountUpdate(db)
        .DeleteUserAccountAsync(userId);

    if (!deleted)
    {
        return Results.NotFound(
            "Kontoen blev ikke fundet."
        );
    }

    // Delete the uploaded profile image
    if (!string.IsNullOrWhiteSpace(imagePath) &&
        imagePath.StartsWith(
            "/profile-images/",
            StringComparison.Ordinal) &&
        !string.IsNullOrWhiteSpace(environment.WebRootPath))
    {
        string fileName = Path.GetFileName(imagePath);

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            string filePath = Path.Combine(
                environment.WebRootPath,
                "profile-images",
                fileName
            );

            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
            }
            catch (IOException ex)
            {
                // Log image deletion failures
                app.Logger.LogWarning(
                    ex,
                    "Could not remove deleted profile image."
                );
            }
            catch (UnauthorizedAccessException ex)
            {
                app.Logger.LogWarning(
                    ex,
                    "Could not remove deleted profile image."
                );
            }
        }
    }

    // Remove the authentication cookie
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    // Redirect the user to the login page
    return Results.LocalRedirect("/login");
})
.RequireAuthorization();

app.Run();
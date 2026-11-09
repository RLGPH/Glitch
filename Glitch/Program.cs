
using Glitch.Components;
using Glitch.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;

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

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Læs brugerens login-cookie
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();


app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();


// ==========================================
// LOG UD
// ==========================================

// Modtager POST fra Log ud-knappen.
// Bruger den eksisterende cookie-authentication.

app.MapPost("/auth/logout", async (HttpContext context) =>
{
    // Fjern brugerens login-cookie.
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    // Send brugeren tilbage til login.
    return Results.LocalRedirect("/login");

})
.RequireAuthorization();


// Start applikationen.
app.Run();



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

// Logout
app.MapPost("/auth/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    return Results.LocalRedirect("/login");
})
.RequireAuthorization();

app.Run();


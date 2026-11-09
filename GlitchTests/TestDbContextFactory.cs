using Glitch.Data;
using Microsoft.EntityFrameworkCore;

namespace GlitchTests;

internal sealed class TestDbContextFactory : IDbContextFactory<GlitchDbContext>
{
    private readonly DbContextOptions<GlitchDbContext> _options;

    public TestDbContextFactory()
    {
        _options = new DbContextOptionsBuilder<GlitchDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    public GlitchDbContext CreateDbContext()
    {
        return new GlitchDbContext(_options);
    }

    public Task<GlitchDbContext> CreateDbContextAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(CreateDbContext());
    }
}
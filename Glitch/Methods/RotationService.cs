using Glitch.Data;
using Microsoft.EntityFrameworkCore;

namespace Glitch.Methods;

public class RotationService
{
    private readonly GlitchDbContext _context;

    public RotationService(GlitchDbContext context) => _context = context;

    /// <summary>
    /// Requirement F21: Resets the active swiping view loop by purging positive Likes that haven't matched yet
    /// </summary>
    public async Task ResetViewingRoundAsync(int profileId)
    {
        // Gather historical likes that didn't materialize into matches
        var nonMatchedLikes = await _context.Swipes
            .Where(s => s.FromProfileID == profileId && s.IsLiked)
            .Where(s => !_context.Matches.Any(m => 
                (m.ProfileAID == profileId && m.ProfileBID == s.ToProfileID) || 
                (m.ProfileAID == s.ToProfileID && m.ProfileBID == profileId)))
            .ToListAsync();

        // Wipe them out to recycle those profiles back into rotation for the next round
        _context.Swipes.RemoveRange(nonMatchedLikes);
        await _context.SaveChangesAsync();
    }
}
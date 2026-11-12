using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Glitch.Data;

namespace Glitch.Methods;

public class ProfileManagementService
{
    private readonly GlitchDbContext _context;

    public ProfileManagementService(GlitchDbContext context) => _context = context;


    public async Task<bool> DeleteUserAccountAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return false;
        int profileId = user.Profile.ProfileID;

        // 1. Clear out join table assignments
        var conditions = _context.ProfileConditions.Where(pc => pc.Profilefield == profileId);
        _context.ProfileConditions.RemoveRange(conditions);

        var interests = _context.ProfileInterests.Where(pi => pi.ProfileID == profileId);
        _context.ProfileInterests.RemoveRange(interests);

        // 2. Erase swiping history records
        var swipes = _context.Swipes.Where(s => s.FromProfileID == profileId || s.ToProfileID == profileId);
        _context.Swipes.RemoveRange(swipes);

        // 3. ADD MESSAGE DELETION RIGHT HERE (Before Matches and Profile!)
        var messages = _context.Messages
            .Where(m => m.SenderProfileID == profileId || m.ReciverProfileID == profileId);
        _context.Messages.RemoveRange(messages);

        // 4. Clear matches
        var matches = _context.Matches.Where(m => m.ProfileAID == profileId || m.ProfileBID == profileId);
        _context.Matches.RemoveRange(matches);

        // 5. Wipe core account records last
        _context.Profiles.Remove(user.Profile);
        _context.Users.Remove(user);

        await _context.SaveChangesAsync();
        return true;
    }
}
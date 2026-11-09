using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;

namespace Glitch.Methods;

public class Matchmaking_service
{
    private readonly GlitchDbContext _context;

    public Matchmaking_service(GlitchDbContext context)
    {
        _context = context;
    }
    
    /// <param name="currentProfileId">The logged-in user's profile ID.</param>
    /// <param name="filterConditionIds">Conditions selected in the UI filter panel. If empty, condition filters are ignored.</param>
    /// <param name="filterInterestIds">Interests selected in the UI filter panel. If empty, interest filters are ignored.</param>
    /// <param name="isResetRoundPressed">Set to true when the user hits 'Start forfra'. Temporarily bypasses session history while preserving active cooldowns.</param>
    public async Task<List<Profile>> GetPotentialMatchesAsync(
        int currentProfileId, 
        List<int> filterConditionIds, 
        List<int> filterInterestIds, 
        bool isResetRoundPressed = false)
    {
        var excludedProfileIds = new List<int> { currentProfileId };
        
        var activeMatchIds = await _context.Matches
            .Where(m => m.ProfileAID == currentProfileId || m.ProfileBID == currentProfileId)
            .Select(m => m.ProfileAID == currentProfileId ? m.ProfileBID : m.ProfileAID)
            .ToListAsync();
        excludedProfileIds.AddRange(activeMatchIds);

        var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
        var recentRejections = await _context.Swipes
            .Where(s => s.FromProfileID == currentProfileId && !s.IsLiked && s.CreatedAt > sevenDaysAgo)
            .Select(s => s.ToProfileID)
            .ToListAsync();
        excludedProfileIds.AddRange(recentRejections);
        
        if (!isResetRoundPressed)
        {
            var historicalSwipes = await _context.Swipes
                .Where(s => s.FromProfileID == currentProfileId)
                .Select(s => s.ToProfileID)
                .ToListAsync();
            excludedProfileIds.AddRange(historicalSwipes);
        }
        
        var query = _context.Profiles.Where(p => !excludedProfileIds.Contains(p.ProfileID));
        
        if (filterConditionIds != null && filterConditionIds.Any())
        {
            foreach (var condId in filterConditionIds)
            {
                query = query.Where(p => _context.ProfileConditions
                    .Any(pc => pc.Profilefield == p.ProfileID && pc.ConditionID == condId));
            }
        }
        
        if (filterInterestIds != null && filterInterestIds.Any())
        {
            foreach (var intId in filterInterestIds)
            {
                query = query.Where(p => _context.ProfileInterests
                    .Any(pi => pi.ProfileID == p.ProfileID && pi.InterestID == intId));
            }
        }

        return await query.OrderByDescending(p => p.LastUpdatedAt).ToListAsync();
    }
}
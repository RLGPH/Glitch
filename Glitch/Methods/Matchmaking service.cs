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

    /// <summary>
    /// Fetches ranked potential matches straight from the database.
    /// </summary>
    public async Task<List<ProfileMatchScore>> GetPotentialMatchesAsync(int currentProfileId)
    {
        // 1. Find all profile IDs the user has already swiped on
        var excludedProfileIds = await _context.Swipes
            .Where(s => s.FromPrifileID == currentProfileId) // Matches your model's FromPrifileID typo
            .Select(s => s.ToProfileID)
            .ToListAsync();

        // Add current user to exclusions so they don't match with themselves
        excludedProfileIds.Add(currentProfileId);

        // 2. Extract lists of the current user's condition and interest IDs to use in our matching query
        var currentUserConditionIds = await _context.ProfileConditions
            .Where(pc => pc.Profilefield == currentProfileId) // Matches your Profilefield typo
            .Select(pc => pc.ConditionID)
            .ToListAsync();

        var currentUserInterestIds = await _context.ProfileInterests
            .Where(pi => pi.ProfileID == currentProfileId)
            .Select(pi => pi.InterestID)
            .ToListAsync();

        // 3. Query profiles directly in SQL, calculating scores dynamically
        var potentialMatchesQuery = _context.Profiles
            .Where(p => !excludedProfileIds.Contains(p.ProfileID))
            .Select(otherProfile => new ProfileMatchScore
            {
                Profile = otherProfile,
                
                // Count how many conditions match the current user
                SharedConditionsCount = _context.ProfileConditions
                    .Count(pc => pc.Profilefield == otherProfile.ProfileID && currentUserConditionIds.Contains(pc.ConditionID)),

                // Count how many interests match the current user
                SharedInterestsCount = _context.ProfileInterests
                    .Count(pi => pi.ProfileID == otherProfile.ProfileID && currentUserInterestIds.Contains(pi.InterestID)),

                // Dynamic scoring (10pts per shared condition, 5pts per shared interest)
                MatchScore = (_context.ProfileConditions.Count(pc => pc.Profilefield == otherProfile.ProfileID && currentUserConditionIds.Contains(pc.ConditionID)) * 10) +
                             (_context.ProfileInterests.Count(pi => pi.ProfileID == otherProfile.ProfileID && currentUserInterestIds.Contains(pi.InterestID)) * 5)
            })
            .OrderByDescending(m => m.MatchScore)         // Highest scores first
            .ThenByDescending(m => m.Profile.LastUpdatedAt); // Tie-breaker: active profiles first

        return await potentialMatchesQuery.ToListAsync();
    }
}

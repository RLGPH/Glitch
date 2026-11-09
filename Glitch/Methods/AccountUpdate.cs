using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;

namespace Glitch.Methods;

public class AccountUpdate
{
    private readonly GlitchDbContext _context;

    public AccountUpdate(GlitchDbContext context)
    {
        _context = context;
    }
    
    public async Task<bool> UpdateProfileBasicsAsync(int profileId, string displayName, string bio, string imagePath)
    {
        var profile = await _context.Profiles.FirstOrDefaultAsync(p => p.ProfileID == profileId);
        if (profile == null) return false;

        profile.DisplayName = displayName;
        profile.Bio = bio;
        profile.ImagePath = imagePath;
        profile.LastUpdatedAt = DateTime.UtcNow;

        _context.Profiles.Update(profile);
        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> UpdateProfileAttributesAsync(int profileId, List<int> selectedConditionIds, List<string> customConditions, List<int> selectedInterestIds)
    {
        // 1. Clear out existing assignments
        var oldConditions = _context.ProfileConditions.Where(pc => pc.Profilefield == profileId);
        _context.ProfileConditions.RemoveRange(oldConditions);

        var oldInterests = _context.ProfileInterests.Where(pi => pi.ProfileID == profileId);
        _context.ProfileInterests.RemoveRange(oldInterests);

        // 2. Add verified standard conditions
        foreach (var conditionId in selectedConditionIds)
        {
            _context.ProfileConditions.Add(new ProfileCondition
            {
                Profilefield = profileId,
                ConditionID = conditionId
            });
        }

        // 3. Handle custom text entries saved to the "Andet" category
        foreach (var customName in customConditions.Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            // Find existing or seed a new custom condition item globally
            var conditionItem = await _context.Conditions
                .FirstOrDefaultAsync(c => c.Name.ToLower() == customName.ToLower() && c.Category == "Andet");

            if (conditionItem == null)
            {
                conditionItem = new Condition
                {
                    Name = customName,
                    Category = "Andet",
                    IsCustom = true
                };
                _context.Conditions.Add(conditionItem);
                await _context.SaveChangesAsync();
            }

            _context.ProfileConditions.Add(new ProfileCondition
            {
                Profilefield = profileId,
                ConditionID = conditionItem.ID
            });
        }

        // 4. Add selected standard interests
        foreach (var interestId in selectedInterestIds)
        {
            _context.ProfileInterests.Add(new ProfileInterest
            {
                ProfileID = profileId,
                InterestID = interestId
            });
        }

        return await _context.SaveChangesAsync() > 0;
    }
    
    public async Task<bool> DeleteUserAccountAsync(int userId)
    {
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user == null) return false;
        int profileId = user.Profile.ProfileID;

        // Find mutual matches where this user is participant A or B
        var relatedMatches = await _context.Matches
            .Where(m => m.ProfileAID == profileId || m.ProfileBID == profileId)
            .ToListAsync();

        if (relatedMatches.Any())
        {
            var matchIds = relatedMatches.Select(m => m.ID).ToList();
            
            var messages = await _context.Messages
                .Where(msg => msg.SenderProfileID == profileId)
                .ToListAsync();
            
            _context.Messages.RemoveRange(messages);
            _context.Matches.RemoveRange(relatedMatches);
        }
        
        var swipes = await _context.Swipes
            .Where(s => s.FromProfileID == profileId || s.ToProfileID == profileId)
            .ToListAsync();
        _context.Swipes.RemoveRange(swipes);
        
        var conditions = _context.ProfileConditions.Where(pc => pc.Profilefield == profileId);
        _context.ProfileConditions.RemoveRange(conditions);

        var interests = _context.ProfileInterests.Where(pi => pi.ProfileID == profileId);
        _context.ProfileInterests.RemoveRange(interests);
        
        _context.Profiles.Remove(user.Profile);
        _context.Users.Remove(user);

        return await _context.SaveChangesAsync() > 0;
    }
}

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

    public async Task<bool> UpdateProfileBasicsAsync(
        int profileId,
        string displayName,
        string bio,
        string imagePath)
    {
        var profile = await _context.Profiles
            .FirstOrDefaultAsync(p => p.ProfileID == profileId);

        if (profile == null)
            return false;

        profile.DisplayName = displayName;
        profile.Bio = bio;
        profile.ImagePath = imagePath;
        profile.LastUpdatedAt = DateTime.UtcNow;

        _context.Profiles.Update(profile);

        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> UpdateProfileAttributesAsync(
        int profileId,
        List<int> selectedConditionIds,
        List<string> customConditions,
        List<int> selectedInterestIds)
    {
        // 1. Clear out existing assignments
        var oldConditions = _context.ProfileConditions
            .Where(pc => pc.Profilefield == profileId);

        _context.ProfileConditions.RemoveRange(oldConditions);

        var oldInterests = _context.ProfileInterests
            .Where(pi => pi.ProfileID == profileId);

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
        foreach (var customName in customConditions
            .Where(c => !string.IsNullOrWhiteSpace(c)))
        {
            // Find existing or seed a new custom condition item globally
            var conditionItem = await _context.Conditions
                .FirstOrDefaultAsync(c =>
                    c.Name.ToLower() == customName.ToLower() &&
                    c.Category == "Andet");

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

    // Delete the user's account and all related data
    public async Task<bool> DeleteUserAccountAsync(int userId)
    {
        // Find the account and its profile
        var user = await _context.Users
            .Include(u => u.Profile)
            .FirstOrDefaultAsync(u => u.UserID == userId);

        if (user?.Profile is null)
            return false;

        int profileId = user.Profile.ProfileID;

        // Keep all database changes in a single transaction
        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        // Delete messages sent and received by this profile
        var messages = await _context.Messages
            .Where(msg =>
                msg.SenderProfileID == profileId ||
                msg.ReceiverProfileID == profileId)
            .ToListAsync();

        _context.Messages.RemoveRange(messages);

        // Delete all matches involving this profile
        var relatedMatches = await _context.Matches
            .Where(m =>
                m.ProfileAID == profileId ||
                m.ProfileBID == profileId)
            .ToListAsync();

        _context.Matches.RemoveRange(relatedMatches);

        // Delete all swipes involving this profile
        var swipes = await _context.Swipes
            .Where(s =>
                s.FromProfileID == profileId ||
                s.ToProfileID == profileId)
            .ToListAsync();

        _context.Swipes.RemoveRange(swipes);

        // Delete the profile's condition assignments
        var conditions = await _context.ProfileConditions
            .Where(pc => pc.Profilefield == profileId)
            .ToListAsync();

        _context.ProfileConditions.RemoveRange(conditions);

        // Delete the profile's interest assignments
        var interests = await _context.ProfileInterests
            .Where(pi => pi.ProfileID == profileId)
            .ToListAsync();

        _context.ProfileInterests.RemoveRange(interests);

        // Delete the profile and user account
        _context.Profiles.Remove(user.Profile);
        _context.Users.Remove(user);

        // Save all changes
        await _context.SaveChangesAsync();

        // Commit the transaction
        await transaction.CommitAsync();

        return true;
    }
}
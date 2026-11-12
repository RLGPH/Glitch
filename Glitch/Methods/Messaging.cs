using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;

namespace Glitch.Methods;

public class Messaging
{
    private readonly GlitchDbContext _context;

    public Messaging(GlitchDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsChatAllowedAsync(int currentProfileId, int targetProfileId)
    {
        // Verify that a match exists between the two profiles
        bool matchExists = await _context.Matches.AnyAsync(m =>
            (m.ProfileAID == currentProfileId && m.ProfileBID == targetProfileId) ||
            (m.ProfileAID == targetProfileId && m.ProfileBID == currentProfileId));

        if (!matchExists)
        {
            return false;
        }

        // Verify that both profiles still have an active Like
        bool currentLikesTarget = await _context.Swipes.AnyAsync(s =>
            s.FromProfileID == currentProfileId &&
            s.ToProfileID == targetProfileId &&
            s.IsLiked);

        bool targetLikesCurrent = await _context.Swipes.AnyAsync(s =>
            s.FromProfileID == targetProfileId &&
            s.ToProfileID == currentProfileId &&
            s.IsLiked);

        return currentLikesTarget && targetLikesCurrent;
    }

    public async Task<List<Message>> GetChatHistoryAsync(int currentProfileId, int targetProfileId)
    {
        //Guard check to ensure unauthorized users cannot peek into chats
        if (!await IsChatAllowedAsync(currentProfileId, targetProfileId))
        {
            return new List<Message>();
        }

        //Fetch all messages sent between these two profiles
        return await _context.Messages
            .Where(msg =>
                (msg.SenderProfileID == currentProfileId &&
                 msg.ReceiverProfileID == targetProfileId) ||

                (msg.SenderProfileID == targetProfileId &&
                 msg.ReceiverProfileID == currentProfileId))
            .OrderBy(msg => msg.SentAt)
            .ToListAsync();
    }

    public async Task<Message?> SaveMessageAsync(int senderProfileId, int receiverProfileId, string text)
    {
        //Enforce match security before persisting records
        if (!await IsChatAllowedAsync(senderProfileId, receiverProfileId))
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var newMessage = new Message
        {
            SenderProfileID = senderProfileId,
            ReceiverProfileID = receiverProfileId,
            Text = text.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        return newMessage;
    }
}
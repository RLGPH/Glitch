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
        return await _context.Matches.AnyAsync(m => 
            (m.ProfileAID == currentProfileId && m.ProfileBID == targetProfileId) ||
            (m.ProfileAID == targetProfileId && m.ProfileBID == currentProfileId));
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
                (msg.SenderProfileID == currentProfileId && _context.Swipes.Any(s => s.FromProfileID == currentProfileId && s.ToProfileID == targetProfileId)) || 
                (msg.SenderProfileID == targetProfileId && _context.Swipes.Any(s => s.FromProfileID == targetProfileId && s.ToProfileID == currentProfileId))
            )
            
            .Where(msg => msg.SenderProfileID == currentProfileId || msg.SenderProfileID == targetProfileId)
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
            Text = text,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        return newMessage;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;

namespace Glitch.Hubs;

public class ChatHub : Hub
{
    private readonly GlitchDbContext _context;

    public ChatHub(GlitchDbContext context) => _context = context;
    
    public async Task SendMessage(int senderProfileId, int reciverProfileId, string text)
    {
        // Verify active match exists between sender and receiver
        bool isMatched = await _context.Matches.AnyAsync(m => 
            (m.ProfileAID == senderProfileId && m.ProfileBID == reciverProfileId) || 
            (m.ProfileAID == reciverProfileId && m.ProfileBID == senderProfileId));

        if (!isMatched)
        {
            throw new HubException("Access Denied: You can only message active matches.");
        }

        // Persist message to the database
        var newMessage = new Message
        {
            SenderProfileID = senderProfileId,
            ReciverProfileID = reciverProfileId,
            Text = text,
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        _context.Messages.Add(newMessage);
        await _context.SaveChangesAsync();

        // Broadcast to connected clients
        await Clients.All.SendAsync("ReceiveMessage", newMessage);
    }
    
    public async Task<List<Message>> GetChatHistoryAsync(int profileAId, int profileBId)
    {
        return await _context.Messages
            .Where(m => (m.SenderProfileID == profileAId && m.ReciverProfileID == profileBId) ||
                        (m.SenderProfileID == profileBId && m.ReciverProfileID == profileAId))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
}

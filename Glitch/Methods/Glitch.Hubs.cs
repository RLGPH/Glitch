using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Glitch.Classes;
using Glitch.Data;

namespace Glitch.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IDbContextFactory<GlitchDbContext> _dbFactory;

    public ChatHub(IDbContextFactory<GlitchDbContext> dbFactory)
    {
        _dbFactory = dbFactory;
    }

    // Hent profil-ID fra den indloggede brugers cookie
    private int GetCurrentProfileId()
    {
        var profileId = Context.User?.FindFirst("ProfileID")?.Value;

        if (!int.TryParse(profileId, out int id))
        {
            throw new HubException("Brugeren er ikke logget ind.");
        }

        return id;
    }

    // Tilføj brugeren til sin egen SignalR-gruppe
    public override async Task OnConnectedAsync()
    {
        int profileId = GetCurrentProfileId();

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            $"profile-{profileId}"
        );

        await base.OnConnectedAsync();
    }

    // Kontrollér, at profilerne har et aktivt match
    private static async Task<bool> HasActiveMatchAsync(
        GlitchDbContext db,
        int profileAId,
        int profileBId)
    {
        bool matchExists = await db.Matches.AnyAsync(m =>
            (m.ProfileAID == profileAId && m.ProfileBID == profileBId) ||
            (m.ProfileAID == profileBId && m.ProfileBID == profileAId));

        if (!matchExists)
        {
            return false;
        }

        // Begge profiler skal stadig have et aktivt Like
        bool aLikesB = await db.Swipes.AnyAsync(s =>
            s.FromProfileID == profileAId &&
            s.ToProfileID == profileBId &&
            s.IsLiked);

        bool bLikesA = await db.Swipes.AnyAsync(s =>
            s.FromProfileID == profileBId &&
            s.ToProfileID == profileAId &&
            s.IsLiked);

        return aLikesB && bLikesA;
    }

    // Send en besked til et aktivt match
    public async Task SendMessage(int receiverProfileId, string text)
    {
        int senderProfileId = GetCurrentProfileId();

        if (string.IsNullOrWhiteSpace(text))
        {
            throw new HubException("Beskeden må ikke være tom.");
        }

        await using var db = await _dbFactory.CreateDbContextAsync();

        bool isMatched = await HasActiveMatchAsync(
            db,
            senderProfileId,
            receiverProfileId
        );

        if (!isMatched)
        {
            throw new HubException(
                "Du kan kun sende beskeder til aktive matches."
            );
        }

        var newMessage = new Message
        {
            SenderProfileID = senderProfileId,
            ReceiverProfileID = receiverProfileId,
            Text = text.Trim(),
            SentAt = DateTime.UtcNow,
            IsRead = false
        };

        db.Messages.Add(newMessage);
        await db.SaveChangesAsync();

        // Send kun til de to profiler i samtalen
        await Clients.Groups(
            $"profile-{senderProfileId}",
            $"profile-{receiverProfileId}"
        ).SendAsync("ReceiveMessage", new
        {
            newMessage.ID,
            newMessage.SenderProfileID,
            newMessage.ReceiverProfileID,
            newMessage.Text,
            newMessage.SentAt
        });
    }

    // Hent historikken mellem brugeren og et aktivt match
    public async Task<List<Message>> GetChatHistoryAsync(int otherProfileId)
    {
        int currentProfileId = GetCurrentProfileId();

        await using var db = await _dbFactory.CreateDbContextAsync();

        bool isMatched = await HasActiveMatchAsync(
            db,
            currentProfileId,
            otherProfileId
        );

        if (!isMatched)
        {
            throw new HubException(
                "Du har ikke adgang til denne samtale."
            );
        }

        return await db.Messages
            .AsNoTracking()
            .Where(m =>
                (m.SenderProfileID == currentProfileId &&
                 m.ReceiverProfileID == otherProfileId) ||
                (m.SenderProfileID == otherProfileId &&
                 m.ReceiverProfileID == currentProfileId))
            .OrderBy(m => m.SentAt)
            .ToListAsync();
    }
}
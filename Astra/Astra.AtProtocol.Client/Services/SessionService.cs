using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Actor;

namespace Astra.AtProtocol.Client.Services;

public class SessionService(ATProtocol protocol) : BaseService(protocol), ISessionService
{
    private static AtSessionResult? CurrentSession { get; set; }
    
    public async Task<AtSessionResult> LoginWithPassword(string identifier, string password, CancellationToken token)
    {
        var (session, error) = await Protocol.AuthenticateWithPasswordResultAsync(
            identifier: identifier,
            password: password,
            cancellationToken: token);

        if (session == null)
        {
            return new AtSessionResult(
                success: false,
                message: $"Failed to login with password: {error}");
        }
        
        var sessionProfile = await Protocol.Actor.GetProfileAsync(session.Handle, token);
        
        CurrentSession = new AtSessionResult(
            success: true,
            message: $"Successfully logged in ({session.Email ?? "Invalid email"})",
            email: session.Email,
            handle: session.Handle.Handle,
            didHandler: session.Did.Handler,
            didType: session.Did.Type,
            token: session.AccessJwt,
            profile: sessionProfile.Value as ProfileViewDetailed
        );
        
        return CurrentSession;
    }

    public AtSessionResult? GetCurrentSession() => CurrentSession;
}
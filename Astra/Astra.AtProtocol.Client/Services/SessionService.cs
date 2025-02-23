using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using FishyFlip;

namespace Astra.AtProtocol.Client.Services;

public class SessionService(ATProtocol protocol) : BaseService(protocol), ISessionService
{
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

        return new AtSessionResult(
            success: true,
            message: $"Successfully logged in ({session.Email ?? "Invalid email"})",
            email: session.Email,
            handle: session.Handle.Handle,
            didHandler: session.Did.Handler,
            didType: session.Did.Type,
            token: session.AccessJwt
        );
    }
}
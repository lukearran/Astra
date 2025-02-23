using Astra.AtProtocol.Client.Models;

namespace Astra.AtProtocol.Client.Interfaces;

public interface ISessionService
{
    Task<AtSessionResult> LoginWithPassword(string identifier, string password, CancellationToken token);
}
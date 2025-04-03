using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Credentials;

namespace Astra.AtProtocol.Common.Providers.Credentials;

[Obsolete("LocalMemoryCredentialProvider is for testing purposes only and will be removed in a future version.")]
public class LocalMemoryCredentialProvider : ICredentialProvider
{
    private AtCredential? _credential;
    
    public Task<AtCredential?> GetCredential()
    {
        return Task.FromResult(_credential);
    }

    public Task<bool> SetCredential(AtCredential credential)
    {
        _credential = credential;

        return Task.FromResult(true);
    }
}
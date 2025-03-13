using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Credentials;

namespace Astra.AtProtocol.Common.Providers.Credentials;

[Obsolete("LocalMemoryCredentialProvider is for testing purposes only and will be removed in a future version.")]
public class LocalMemoryCredentialProvider : ICredentialProvider
{
    private AtCredential? _credential;
    
    public AtCredential? GetCredential()
    {
        return _credential;
    }

    public bool SetCredential(AtCredential credential)
    {
        _credential = credential;

        return true;
    }
}
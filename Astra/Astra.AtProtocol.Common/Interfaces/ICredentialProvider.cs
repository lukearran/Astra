using Astra.AtProtocol.Common.Models.Credentials;

namespace Astra.AtProtocol.Common.Interfaces;

public interface ICredentialProvider
{
    public AtCredential? GetCredential();
    
    public bool SetCredential(AtCredential credential);
}
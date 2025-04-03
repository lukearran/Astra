using Astra.AtProtocol.Common.Models.Credentials;

namespace Astra.AtProtocol.Common.Interfaces;

public interface ICredentialProvider
{
    public Task<AtCredential?> GetCredential();
    
    public Task<bool> SetCredential(AtCredential credential);
}
namespace Astra.AtProtocol.Common.Providers;

public interface ICredentialProvider
{
    string? Username { get; }
    string? Password { get; }
}
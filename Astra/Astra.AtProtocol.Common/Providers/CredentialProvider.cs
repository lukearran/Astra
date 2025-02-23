namespace Astra.AtProtocol.Common.Providers;

public class CredentialProvider(string? defaultUsername = null, string? defaultPassword = null) : ICredentialProvider
{
    public string? Username { get; } = defaultUsername;
    public string? Password { get; } = defaultPassword;
}
namespace Astra.AtProtocol.Common.Models.Credentials;

public class AtCredential(string username, string password)
{
    public string Username { get; set; } = username;
    public string Password { get; set; } = password;
}
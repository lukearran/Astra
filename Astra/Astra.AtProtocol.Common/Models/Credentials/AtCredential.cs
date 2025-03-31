using System.Text.Json;

namespace Astra.AtProtocol.Common.Models.Credentials;

public class AtCredential(string username, string password)
{
    public string Username { get; } = username;
    public string Password { get; } = password;

    public string ToJson()
    {
        return JsonSerializer.Serialize(this);
    }
}
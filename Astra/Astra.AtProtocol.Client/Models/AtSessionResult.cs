using FishyFlip.Lexicon.App.Bsky.Actor;

namespace Astra.AtProtocol.Client.Models;

public class AtSessionResult(
    bool success,
    string message,
    string? email = null,
    string? handle = null,
    string? didHandler = null,
    string? didType = null,
    string? token = null,
    ProfileViewDetailed? profile = null) : BaseResult(success, message)
{
    public string? Email { get; } = email;
    public string? Handle { get; } = handle;
    public string? DidHandler { get; } = didHandler;
    public string? DidType { get; } = didType;
    public string? Token { get; } = token;
    public ProfileViewDetailed? ProfileView { get; } = profile;
}
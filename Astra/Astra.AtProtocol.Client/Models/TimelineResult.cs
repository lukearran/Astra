using FishyFlip.Lexicon.App.Bsky.Feed;

namespace Astra.AtProtocol.Client.Models;

public class TimelineResult(
    bool success,
    string message,
    List<FeedViewPost>? posts = null) : BaseResult(success, message)
{
    public List<FeedViewPost> Posts { get; } = posts ?? [];
}
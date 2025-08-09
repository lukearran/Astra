using FishyFlip.Lexicon.App.Bsky.Feed;

namespace Astra.AtProtocol.Client.Models;

public class ThreadResult(bool success, string message, ThreadViewPost? post = null) : BaseResult(success, message)
{
    public ThreadViewPost? Parent { get; } = post?.Parent as ThreadViewPost;
    public PostView? Post { get; } = post?.Post;
    public List<ThreadViewPost> Replies { get; } = post?
        .Replies?
        .Cast<ThreadViewPost>()
        .ToList() ?? [];
}
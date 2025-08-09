using Astra.AtProtocol.Client.Models;

namespace Astra.AtProtocol.Client.Interfaces;

public interface IUserFeedService
{
    Task<ThreadResult> GetPostThread(string postUri);
    
    Task<FeedResult> GetFollowingFeed(int limit, CancellationToken token, string? cursor = null);

    Task<FeedResult> GetFeed(string feedUri, int limit, CancellationToken token, string? cursor = null);

    Task<AtFeedMeta> GetFeedDetail(AtFeed feed);
}
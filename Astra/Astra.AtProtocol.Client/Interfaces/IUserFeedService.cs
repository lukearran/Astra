using Astra.AtProtocol.Client.Models;

namespace Astra.AtProtocol.Client.Interfaces;

public interface IUserFeedService
{
    Task<FeedResult> GetFollowingFeed(int limit, CancellationToken token, string? cursor = null);

    Task<FeedResult> GetFeed(string feedUri, int limit, CancellationToken token, string? cursor = null);

    Task<AtFeedMeta> GetFeedDetail(AtFeed feed);
}
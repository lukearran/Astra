using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using Astra.AtProtocol.Common;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;
using FishyFlip.Models;

namespace Astra.AtProtocol.Client.Services;

public class UserFeedService(ATProtocol protocol) : BaseService(protocol), IUserFeedService
{
    public async Task<ThreadResult> GetPostThread(string postUri)
    {
        var result = 
            await Protocol.Feed.GetPostThreadAsync(ATUri.Create(postUri));

        var thread = (result.Value as GetPostThreadOutput)?.Thread;

        if (thread is not ThreadViewPost view)
        {
            return new ThreadResult(
                success: false,
                message: $"Failed to fetch post: {postUri}");
        }
        
        return new ThreadResult(
            success: true,
            message: $"Successfully fetched post: {postUri}",
            post: view);
    }
    
    public async Task<FeedResult> GetFollowingFeed(int limit, CancellationToken token, string? cursor = null)
    {
        var (timelineOutputResult, error) = await Protocol.GetTimelineAsync(
            limit: limit,
            cancellationToken: token,
            cursor: cursor);
        
        if (timelineOutputResult is null)
        {
            return new FeedResult(
                success: false,
                message: $"Failed to get following feed: {error}");
        }

        return new FeedResult(
            success: true,
            message: $"Successfully fetched following feed - total {timelineOutputResult.Feed?.Count}",
            posts: timelineOutputResult.Feed,
            cursor: timelineOutputResult.Cursor);
    }

    public async Task<FeedResult> GetFeed(string feedUri, int limit, CancellationToken token, string? cursor = null)
    {
        var (timelineOutputResult, error) = await Protocol.GetFeedAsync(
            feed: new ATUri(feedUri),
            limit: limit,
            cancellationToken: token,
            cursor: cursor);
        
        if (timelineOutputResult is null)
        {
            return new FeedResult(
                success: false,
                message: $"Failed to get feed ({feedUri}): {error}");
        }

        return new FeedResult(
            success: true,
            message: $"Successfully fetched feed ({feedUri}) - total {timelineOutputResult.Feed?.Count}",
            posts: timelineOutputResult.Feed,
            cursor: timelineOutputResult.Cursor);
    }

    public async Task<AtFeedMeta> GetFeedDetail(AtFeed feed)
    {
        if (feed.Type == CommonConstants.FollowingFeedTypeName)
        {
            return new AtFeedMeta(displayName: "Following");
        }
        
        var feedDetail = 
            await Protocol.Feed.GetFeedGeneratorAsync(new ATUri(feed.Value));

        if (feedDetail.Value is not GetFeedGeneratorOutput feedDetailOutput)
            throw new InvalidOperationException($"{nameof(GetFeedGeneratorOutput)}: Failed to fetch feed detail");
        
        return new AtFeedMeta(
            displayName: feedDetailOutput.View.DisplayName,
            description: feedDetailOutput.View.Description,
            avatarUrl: feedDetailOutput.View.Avatar);
    }
}
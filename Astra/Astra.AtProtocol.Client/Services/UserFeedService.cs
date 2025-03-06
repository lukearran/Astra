using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using FishyFlip;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace Astra.AtProtocol.Client.Services;

public class UserFeedService(ATProtocol protocol) : BaseService(protocol), IUserFeedService
{
    public async Task<TimelineResult> GetUserTimeline(int limit, CancellationToken token, string? cursor = null)
    {
        var (timelineOutputResult, error) = await Protocol.GetTimelineAsync(
            limit: limit,
            cancellationToken: token,
            cursor: cursor);
        
        if (timelineOutputResult is null)
        {
            return new TimelineResult(
                success: false,
                message: $"Failed to get timeline: {error}");
        }

        return new TimelineResult(
            success: true,
            message: $"Successfully retrieved timeline - total {timelineOutputResult.Feed?.Count}",
            posts: timelineOutputResult.Feed,
            cursor: timelineOutputResult.Cursor);
    }
}
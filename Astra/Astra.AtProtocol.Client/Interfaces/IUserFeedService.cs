using Astra.AtProtocol.Client.Models;

namespace Astra.AtProtocol.Client.Interfaces;

public interface IUserFeedService
{
    Task<TimelineResult> GetUserTimeline(int limit, CancellationToken token, string? cursor = null);
}
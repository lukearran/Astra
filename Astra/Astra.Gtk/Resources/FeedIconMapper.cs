using Astra.AtProtocol.Common;

namespace Astra.Gtk.Resources;

public static class FeedIconMapper
{
    public static string MapIconName(string feedType)
    {
        if (feedType.Contains(CommonConstants.UniqueFollowingFeedReference))
        {
            return "people-symbolic";
        }
        else if (feedType.Contains("app.bsky.feed.generator/whats-hot"))
        {
            return "camera-flash-symbolic";
        }
        else if (feedType.Contains("app.bsky.feed.generator/thevids"))
        {
            return "video-clip-symbolic";
        }

        return "newspaper-symbolic";
    }
}
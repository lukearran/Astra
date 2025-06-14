using FishyFlip.Lexicon.App.Bsky.Actor;

namespace Astra.AtProtocol.Client.Models;

public class AtProfile(ProfileViewDetailed profileView)
{
    public string Handle { get; } = profileView.Handle.Handle;
    public string? DisplayName { get; } = profileView.DisplayName;
    public string? AvatarUrl { get; } = profileView.Avatar;
}

public class AtUserPreferences
{
    public List<AtFeed> Feeds { get; }
    
    public AtUserPreferences(GetPreferencesOutput output)
    {
        var feedSettings = output
            .Preferences
            .SingleOrDefault(x => x.Type == "app.bsky.actor.defs#savedFeedsPrefV2") as SavedFeedsPrefV2;

        Feeds = feedSettings?.Items.Select(x =>
            new AtFeed(id: x.Id, isPinned: x.Pinned, type: x.TypeValue, value: x.Value))
            .ToList() ?? [];
    }
}

public class AtFeed (string id, bool isPinned, string type, string value)
{
    public string Id { get; set; } = id;
    public bool IsPinned { get; set; } = isPinned;
    public string Type { get; set; } = type;
    public string Value { get; set; } = value;
}

public class AtFeedMeta(string displayName, string? description = null, string? avatarUrl = null)
{
    public string DisplayName { get; set; } = displayName;
    public string? Description { get; set; } = description;
    public string? AvatarUrl { get; set; } = avatarUrl;
}
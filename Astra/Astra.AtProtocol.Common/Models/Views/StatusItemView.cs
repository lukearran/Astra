using FishyFlip.Lexicon;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;

namespace Astra.AtProtocol.Common.Models.Views;

public enum StatusItemType
{
    Post,
    QuotedPost
}

public class StatusItemView
{
    /// <summary>
    /// The type of post content
    /// </summary>
    public StatusItemType ItemType { get; }
    /// <summary>
    /// The AT Uri of the given post
    /// </summary>
    public string Uri  { get; }
    /// <summary>
    /// The display name of the post's author
    /// </summary>
    public string AuthorDisplayName { get; }
    /// <summary>
    /// The AT Protocol handle for the post's author
    /// </summary>
    public string UserHandle { get; }
    /// <summary>
    /// The date/time when the post was published at
    /// </summary>
    public DateTime? PublishedAt { get; }
    /// <summary>
    /// The text contents of the post
    /// </summary>
    public string StatusText { get; } = string.Empty;
    /// <summary>
    /// The number of replies which the post has received
    /// </summary>
    public long ReplyCount { get; } = 0;
    /// <summary>
    /// The number of re-posts for the post
    /// </summary>
    public long RepostCount { get; } = 0;
    /// <summary>
    /// The number of links which the post has received
    /// </summary>
    public long LikeCount { get; } = 0;
    /// <summary>
    /// The URL of the author's profile picture
    /// </summary>
    public string ProfilePictureUrl { get; }
    /// <summary>
    /// The AT Object of the embedded content
    /// </summary>
    public ATObject? EmbeddedContent { get; }

    public StatusItemView(PostView post)
    {
        ItemType = StatusItemType.Post;
        Uri = post.Uri.ToString();
        AuthorDisplayName = post.Author.DisplayName ?? string.Empty;
        UserHandle = post.Author.Handle.Handle;
        PublishedAt = post.PostRecord?.CreatedAt;
        StatusText = post.PostRecord?.Text ?? string.Empty;
        ReplyCount = post.ReplyCount ?? 0;
        RepostCount = post.RepostCount ?? 0;
        LikeCount = post.LikeCount ?? 0;
        ProfilePictureUrl = post.Author.Avatar ?? string.Empty;
        EmbeddedContent = post.Embed;
    }

    public StatusItemView(ViewRecord record)
    {
        ItemType = StatusItemType.QuotedPost;
        Uri = record.Uri.ToString();
        ProfilePictureUrl = record.Author.Avatar ?? string.Empty;
        AuthorDisplayName = record.Author.DisplayName ?? string.Empty;
        UserHandle = record.Author.Handle.Handle;
        ReplyCount = record.ReplyCount ?? 0;
        RepostCount = record.RepostCount ?? 0;
        LikeCount = record.LikeCount ?? 0;
        EmbeddedContent = record.Embeds?.SingleOrDefault();

        if (record.Value is Post post)
        {
            PublishedAt = post.CreatedAt;
            StatusText = post.Text ?? string.Empty;
        }
    }
}
using Adw.Internal;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gtk.Internal;
using Builder = Gtk.Builder;
using ListBoxRow = Gtk.ListBoxRow;

namespace Astra.Gtk.Views;

public class StatusItem : ListBoxRow
{
    [global::Gtk.Connect("post_handle")] private readonly global::Gtk.Label? _postHandle;
    [global::Gtk.Connect("post_handle_sub")] private readonly global::Gtk.Label? _postHandleSub;
    [global::Gtk.Connect("post_content")] private readonly global::Gtk.Label? _postContent;
    [global::Gtk.Connect("profile_picture")] private readonly Adw.Avatar? _profilePicture;
    
    public StatusItem(Builder builder, PostView content) : base(new ListBoxRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        
        // Set the handle label
        _postHandle?.SetVisible(string.IsNullOrEmpty(content.Author.DisplayName) is false);
        _postHandle?.SetText(content.Author.DisplayName ?? string.Empty);
        
        // Set the handle sub-content
        var subContent = GetSubHandle(content);
        _postHandleSub?.SetText(subContent);
        
        // TODO: Set profile picture from network URL

        // Set markdown content
        _postContent?.SetText(content.PostRecord?.Text ?? string.Empty);
    }
    
    public StatusItem(PostView content) : this(new Builder("StatusItem.ui"), content)
    {
    }

    private string GetSubHandle(PostView content)
    {
        // Get the profile handle
        var handle = content.Author.Handle.Handle;
        
        // Get the created time
        var relativeTime = content.PostRecord?.CreatedAt;

        return relativeTime != null ? $"{handle} · TODO" : handle;
    }
}
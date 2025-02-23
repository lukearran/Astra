using Adw.Internal;
using Astra.Gtk.Extensions;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gtk.Internal;
using Builder = Gtk.Builder;
using Button = Gtk.Button;
using ListBoxRow = Gtk.ListBoxRow;
using ToggleButton = Gtk.ToggleButton;

namespace Astra.Gtk.Views;

public class StatusItem : ListBoxRow
{
    // Posted By, Handles, and Posted At
    [global::Gtk.Connect("post_handle")] private readonly global::Gtk.Label? _postHandle = null;
    [global::Gtk.Connect("post_handle_sub")] private readonly global::Gtk.Label? _postHandleSub = null;
    [global::Gtk.Connect("profile_picture")] private readonly Adw.Avatar? _profilePicture = null;
    // Content of the post
    [global::Gtk.Connect("post_content")] private readonly global::Gtk.Label? _postContent = null;
    // Buttons
    [global::Gtk.Connect("reply_button")] private readonly Button? _replyButton = null;
    [global::Gtk.Connect("repost_button")] private readonly ToggleButton? _repostButton = null;
    [global::Gtk.Connect("heart_button")] private readonly ToggleButton? _heartButton = null;

    private StatusItem(Builder builder, PostView content) : base(new ListBoxRowHandle(builder.GetPointer("_root"), false))
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
        
        // Register "Reply" button click event
        if (_replyButton != null)
        {
            _replyButton.OnClicked += ReplyButtonOnClicked;
        }
        // Register "Repost" button toggle event
        if (_repostButton != null)
        {
            _repostButton.OnToggled += RepostButtonOnToggled;
        }
        // Register "Heart" (aka like/favorite) button toggle event
        if (_heartButton != null)
        {
            _heartButton.OnToggled += HeartButtonOnToggled;
        }
    }

    public StatusItem(PostView content) : this(new Builder("StatusItem.ui"), content) { }

    private string GetSubHandle(PostView content)
    {
        // Get the profile handle
        var handle = content.Author.Handle.Handle;
        
        // Get the created time
        var createdAt = content.PostRecord?.CreatedAt;

        return createdAt != null ? $"{handle} · {createdAt.Value.ToRelativeDate()}" : handle;
    }
    
    private void HeartButtonOnToggled(ToggleButton sender, EventArgs args)
    {
        SetToggleButtonStyle(sender, "post_love_button_toggled");
    }

    private void RepostButtonOnToggled(ToggleButton sender, EventArgs args)
    {
        SetToggleButtonStyle(sender, "post_repost_button_toggled");
    }

    private void ReplyButtonOnClicked(Button sender, EventArgs args)
    {
        throw new NotImplementedException();
    }
    
    private void SetToggleButtonStyle(ToggleButton sender, string styleClass)
    {
        if (sender.Active)
        {
            sender?.AddCssClass(styleClass);
        }
        else
        {
            sender?.RemoveCssClass(styleClass);
        }
    }
}
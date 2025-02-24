using Astra.Gtk.Extensions;
using Astra.Gtk.Functions;
using Astra.Gtk.Helpers;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gtk.Internal;
using Builder = Gtk.Builder;
using Button = Gtk.Button;
using ListBoxRow = Gtk.ListBoxRow;
using Task = System.Threading.Tasks.Task;
using ToggleButton = Gtk.ToggleButton;

namespace Astra.Gtk.Views;

public class StatusItem : ListBoxRow
{
    // Content model
    private readonly PostView _content;
    // Posted By, Handles, and Posted At
    [global::Gtk.Connect("post_handle")] private readonly global::Gtk.Label? _postHandle = null;
    [global::Gtk.Connect("post_handle_sub")] private readonly global::Gtk.Label? _postHandleSub = null;
    [global::Gtk.Connect("profile_picture")] private readonly Adw.Avatar? _profilePicture = null;
    // Content of the post
    [global::Gtk.Connect("post_content")] private readonly global::Gtk.Label? _postContent = null;
    // Buttons
    [global::Gtk.Connect("reply_button")] private readonly Button? _replyButton = null;
    [global::Gtk.Connect("reply_button_content")] private readonly Adw.ButtonContent? _replyButtonContent = null;
    
    [global::Gtk.Connect("repost_button")] private readonly ToggleButton? _repostButton = null;
    [global::Gtk.Connect("repost_button_content")] private readonly Adw.ButtonContent? _repostButtonContent = null;
    
    [global::Gtk.Connect("heart_button")] private readonly ToggleButton? _heartButton = null;
    [global::Gtk.Connect("heart_button_content")] private readonly Adw.ButtonContent? _heartButtonContent = null;
    // Embedded post content
    [global::Gtk.Connect("embedded_card")] private readonly global::Gtk.Frame? _embeddedContentFrame = null;
    [global::Gtk.Connect("embedded_card_thumbnail")] private readonly global::Gtk.Picture? _embeddedContentThumbnail = null;
    [global::Gtk.Connect("embedded_card_headline")] private readonly global::Gtk.Label? _embeddedContentHeadline = null;
    [global::Gtk.Connect("embedded_card_description")] private readonly global::Gtk.Label? _embeddedContentDescription = null;
    [global::Gtk.Connect("embedded_card_link")] private readonly global::Gtk.Label? _embeddedContentLink = null;

    private StatusItem(Builder builder, PostView content) : base(new ListBoxRowHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        
        // Set models
        _content = content;
        
        // Set the handle label
        _postHandle?.SetVisible(string.IsNullOrEmpty(content.Author.DisplayName) is false);
        _postHandle?.SetText(content.Author.DisplayName ?? string.Empty);
        
        // Set the handle sub-content
        var subContent = GetSubHandle(content);
        _postHandleSub?.SetText(subContent);

        // Set markdown content
        _postContent?.SetVisible(!string.IsNullOrEmpty(content.PostRecord?.Text));
        _postContent?.SetText(content.PostRecord?.Text ?? string.Empty);
        
        // Register "Reply" button click event
        _replyButtonContent?.SetLabel(content.ReplyCount.ToString() ?? "0");
        if (_replyButton != null)
        {
            _replyButton.OnClicked += ReplyButtonOnClicked;
        }
        // Register "Repost" button toggle event
        _repostButtonContent?.SetLabel(content.RepostCount.ToString() ?? "0");
        if (_repostButton != null)
        {
            _repostButton.OnToggled += RepostButtonOnToggled;
        }
        // Register "Heart" (aka like/favorite) button toggle event
        _heartButtonContent?.SetLabel(content.LikeCount.ToString() ?? "0");
        if (_heartButton != null)
        {
            _heartButton.OnToggled += HeartButtonOnToggled;
        }
        
        // Fire and forget setting the profile picture
        if (!string.IsNullOrEmpty(content.Author.Avatar))
        {
            _ = TrySetProfilePicture(content.Author.Avatar);
        }
        
        // Fire and forget setting the embedded content
        if (content.PostRecord?.Embed != null)
        {
            _ = SetEmbeddedContent(content);
        }
    }

    public StatusItem(PostView content) 
        : this(new Builder("StatusItem.ui"), content)
    {
        
    }
    
    private async Task SetEmbeddedContent(PostView content)
    {
        // Ignore, if there is no embed content
        if (content.PostRecord?.Embed == null
            && _embeddedContentFrame == null
            && _embeddedContentThumbnail == null
            && _embeddedContentHeadline == null
            && _embeddedContentDescription == null
            && _embeddedContentLink == null)
        {
            return;
        }
        
        // Get embedded external
        if (content.Embed is not ViewExternal externalEmbedded)
        {
            return;
        }
        
        // Make the frame visible
        _embeddedContentFrame?.SetVisible(true);
        
        // Set the headline
        _embeddedContentHeadline?.SetLabel(externalEmbedded.External.Title.Trim());
        
        // Set the description
        _embeddedContentDescription?.SetLabel(externalEmbedded.External.Description.Trim());
        
        // Set the link
        _embeddedContentLink?.SetLabel(
            UrlHelpers.StripUrlToDomain(externalEmbedded.External.Uri));
        
        // Set the thumbnail
        if (!string.IsNullOrEmpty(externalEmbedded.External.Thumb))
        {
            try
            {
                // Download the external thumbnail
                var thumbnailBytes = await NetworkFunction.GetDataInBytesAsync(externalEmbedded.External.Thumb);

                // Set the picture
                if (thumbnailBytes != null && _embeddedContentThumbnail != null)
                {
                    _embeddedContentThumbnail.Paintable = Gdk.Texture.NewFromBytes(thumbnailBytes);
                }
            }
            catch
            {
                return;
            }
        }
    }

    private async Task TrySetProfilePicture(string profilePictureUrl)
    {
        try
        {
            if (_profilePicture == null)
            {
                return;
            }
            
            // Get the bytes
            var bytes = await NetworkFunction.GetDataInBytesAsync(profilePictureUrl);
            
            if (bytes == null)
            {
                return;
            }
            
            // Set the profile picture
            _profilePicture.CustomImage = Gdk.Texture.NewFromBytes(bytes);
        }
        catch (System.Exception ex)
        {
            // TODO: Get DI logger
            Console.WriteLine("Failed to set profile picture: " + ex.Message);
        }
    }

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
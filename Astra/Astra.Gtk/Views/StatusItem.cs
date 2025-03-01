using Astra.Gtk.Extensions;
using Astra.Gtk.Functions;
using Astra.Gtk.Helpers;
using FishyFlip.Lexicon.App.Bsky.Embed;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gdk;
using Gtk;
using Gtk.Internal;
using Microsoft.Extensions.Logging;
using Builder = Gtk.Builder;
using Button = Gtk.Button;
using Frame = Gtk.Frame;
using GestureClick = Gtk.GestureClick;
using ListBoxRow = Gtk.ListBoxRow;
using Picture = Gtk.Picture;
using Task = System.Threading.Tasks.Task;
using ToggleButton = Gtk.ToggleButton;

namespace Astra.Gtk.Views;

public class StatusItem : ListBoxRow
{
    // Logger
    private readonly ILogger<StatusItem> _logger;

    // Content model
    private readonly PostView _content;

    // Posted By, Handles, and Posted At
    [global::Gtk.Connect("post_handle")] private readonly global::Gtk.Label? _postHandle = null;

    [global::Gtk.Connect("post_handle_sub")]
    private readonly global::Gtk.Label? _postHandleSub = null;

    [global::Gtk.Connect("profile_picture")]
    private readonly Adw.Avatar? _profilePicture = null;

    // Content of the post
    [global::Gtk.Connect("post_content")] private readonly global::Gtk.Label? _postContent = null;

    // Buttons
    [global::Gtk.Connect("reply_button")] private readonly Button? _replyButton = null;

    [global::Gtk.Connect("reply_button_content")]
    private readonly Adw.ButtonContent? _replyButtonContent = null;

    [global::Gtk.Connect("repost_button")] private readonly ToggleButton? _repostButton = null;

    [global::Gtk.Connect("repost_button_content")]
    private readonly Adw.ButtonContent? _repostButtonContent = null;

    [global::Gtk.Connect("heart_button")] private readonly ToggleButton? _heartButton = null;

    [global::Gtk.Connect("heart_button_content")]
    private readonly Adw.ButtonContent? _heartButtonContent = null;

    // Embedded external link content
    [global::Gtk.Connect("embedded_card")] private readonly global::Gtk.Frame? _embeddedContentFrame = null;

    [global::Gtk.Connect("embedded_card_thumbnail")]
    private readonly global::Gtk.Picture? _embeddedContentThumbnail = null;

    [global::Gtk.Connect("embedded_card_headline")]
    private readonly global::Gtk.Label? _embeddedContentHeadline = null;

    [global::Gtk.Connect("embedded_card_description")]
    private readonly global::Gtk.Label? _embeddedContentDescription = null;

    [global::Gtk.Connect("embedded_card_link")]
    private readonly global::Gtk.Label? _embeddedContentLink = null;

    // Embedded photo content
    [global::Gtk.Connect("picture_container_box")]
    private readonly global::Gtk.Box? _pictureContainerBox = null;

    [global::Gtk.Connect("picture_flowbox")]
    private readonly global::Gtk.ScrolledWindow? _pictureScrollWindowContainer = null;

    private StatusItem(ILoggerFactory loggerFactory, Builder builder, PostView content) : base(new ListBoxRowHandle(
        builder.GetPointer("_root"),
        false))
    {
        builder.Connect(this);

        _logger = loggerFactory.CreateLogger<StatusItem>();

        _logger.LogDebug("Creating StatusItem Widget for Post: {Post}", content.Uri);

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

    public StatusItem(PostView content, ILoggerFactory loggerFactory)
        : this(loggerFactory, new Builder("StatusItem.ui"), content)
    {
    }

    private Task SetEmbeddedContent(PostView content)
    {
        // Ignore, if there is no embed content
        if (content.PostRecord?.Embed == null)
        {
            return Task.CompletedTask;
        }

        _ = content.Embed switch
        {
            ViewExternal externalLink => SetExternalLinkContent(externalLink),
            ViewImages images => SetExternalImagesContent(images),
            ViewImage image => SetExternalImagesContent(new ViewImages() { Images = [image] }),
            // TODO: Video
            // TODO: Embedded sub-status (quote)
            _ => Task.CompletedTask
        };
        return Task.CompletedTask;
    }

    private async Task SetExternalImagesContent(ViewImages images)
    {
        if (_pictureContainerBox == null || _pictureScrollWindowContainer == null)
        {
            return;
        }

        _pictureScrollWindowContainer.SetVisible(true);

        foreach (var image in images.Images)
        {
            try
            {
                var pictureBytes = await NetworkFunction.GetDataInBytesAsync(image.Thumb);

                if (pictureBytes == null)
                {
                    continue;
                }

                var pictureContainer = new Frame();
                pictureContainer.AddCssClass("photo_card");
                pictureContainer.Halign = Align.Start;

                var pictureTexture = Texture.NewFromBytes(pictureBytes);
                var pictureWidget = Picture.NewForPaintable(pictureTexture);

                pictureWidget.ContentFit = ContentFit.Cover;
                pictureWidget.Halign = Align.Center;
                pictureWidget.Valign = Align.Center;
                pictureWidget.KeepAspectRatio = false;
                pictureWidget.CanShrink = true;

                pictureContainer.Child = pictureWidget;

                _pictureContainerBox.Append(pictureContainer);
            }
            catch (SystemException ex)
            {
                Console.Write(ex);
            }
        }
    }

    private async Task SetExternalLinkContent(ViewExternal externalContent)
    {
        if (_embeddedContentFrame == null
            && _embeddedContentThumbnail == null
            && _embeddedContentHeadline == null
            && _embeddedContentDescription == null
            && _embeddedContentLink == null)
        {
            return;
        }

        _embeddedContentFrame?.SetVisible(true);

        _embeddedContentHeadline?.SetLabel(externalContent.External.Title.Trim());

        _embeddedContentDescription?.SetLabel(externalContent.External.Description.Trim());

        _embeddedContentLink?.SetLabel(
            externalContent.External.Uri.ParseRegex(
                RegexHelpers.DomainStripRegex()));

        // Make the frame of the embedded link clickable
        var clickController = new GestureClick();
        clickController.OnPressed += (_, _) => SystemFunction.TryOpenUrlInBrowser(externalContent.External.Uri);

        _embeddedContentFrame?.AddController(clickController);

        if (!string.IsNullOrEmpty(externalContent.External.Thumb))
        {
            try
            {
                _embeddedContentThumbnail?.SetVisible(true);

                var thumbnailBytes = await NetworkFunction.GetDataInBytesAsync(externalContent.External.Thumb);

                if (thumbnailBytes != null && _embeddedContentThumbnail != null)
                {
                    _embeddedContentThumbnail.Paintable = Gdk.Texture.NewFromBytes(thumbnailBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle thumbnail image of external content: {ThumbnailUrl}", externalContent.External.Thumb);
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure setting profile picture {ProfilePicture} on status (Url: {Url})",
                _content.Cid, _content.Uri);
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
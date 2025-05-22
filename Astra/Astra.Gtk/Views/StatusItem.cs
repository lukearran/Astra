using Astra.AtProtocol.Common.Models.Views;
using Astra.Gtk.Extensions;
using Astra.Gtk.Functions;
using Astra.Gtk.Helpers;
using FishyFlip.Lexicon.App.Bsky.Embed;
using Gdk;
using Gtk;
using Gtk.Internal;
using Microsoft.Extensions.Logging;
using Box = Gtk.Box;
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
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<StatusItem> _logger;

    // Content model
    private readonly StatusItemView _content;
    
    // Avatar
    [Connect("avatar_container")]
    private readonly Box? _avatarContainer = null;
    
    [Connect("profile_picture")]
    private readonly Adw.Avatar? _profilePicture = null;
    
    [Connect("profile_picture_mini")]
    private readonly Adw.Avatar? _profilePictureMini = null;

    // Posted By, Handles, and Posted At
    [Connect("post_handle")]
    private readonly global::Gtk.Label? _postHandle = null;

    [Connect("post_handle_sub")]
    private readonly global::Gtk.Label? _postHandleSub = null;

    // Content of the post
    [Connect("post_content")]
    private readonly global::Gtk.Label? _postContent = null;
    
    // Embedded external content
    [Connect("embedded_card_link_frame")]
    private readonly Frame? _embeddedContentFrame = null;

    [Connect("external_link_container")]
    private readonly Box? _externalLinkContainer = null;

    [Connect("embedded_card_thumbnail")]
    private readonly Picture? _embeddedContentThumbnail = null;

    [Connect("embedded_card_headline")]
    private readonly global::Gtk.Label? _embeddedContentHeadline = null;

    [Connect("embedded_card_description")]
    private readonly global::Gtk.Label? _embeddedContentDescription = null;

    [Connect("embedded_card_link")]
    private readonly global::Gtk.Label? _embeddedContentLink = null;

    // Embedded photo content
    [Connect("picture_container_box")]
    private readonly Box? _pictureContainerBox = null;

    [Connect("picture_flowbox")]
    private readonly global::Gtk.ScrolledWindow? _pictureScrollWindowContainer = null;
    
    // Embedded record
    [Connect("embedded_card_record_frame")]
    private readonly Frame? _embeddedContentRecordFrame = null;
    
    [Connect("embedded_record_def")]
    private readonly Box? _embeddedRecordDefContainer = null;
    
    // Action bar
    [Connect("action_bar")]
    private readonly Box? _actionBar = null;
    
    [Connect("reply_button")]
    private readonly Button? _replyButton = null;

    [Connect("reply_button_content")]
    private readonly Adw.ButtonContent? _replyButtonContent = null;

    [Connect("repost_button")]
    private readonly ToggleButton? _repostButton = null;

    [Connect("repost_button_content")]
    private readonly Adw.ButtonContent? _repostButtonContent = null;

    [Connect("heart_button")]
    private readonly ToggleButton? _heartButton = null;

    [Connect("heart_button_content")]
    private readonly Adw.ButtonContent? _heartButtonContent = null;

    private StatusItem(ILoggerFactory loggerFactory, Builder builder, StatusItemView content) : base(new ListBoxRowHandle(
        builder.GetPointer("_root"),
        false))
    {
        builder.Connect(this);

        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<StatusItem>();

        _logger.LogDebug("Creating StatusItem Widget for Post: {Post}", content.Uri);

        _content = content;

        // Set the handle label
        _postHandle?.SetVisible(string.IsNullOrEmpty(content.AuthorDisplayName) is false);
        _postHandle?.SetText(content.AuthorDisplayName);

        // Set the handle sub-content
        var subContent = GetSubHandle(content);
        _postHandleSub?.SetText(subContent);

        // Set markdown content
        _postContent?.SetVisible(!string.IsNullOrEmpty(content.StatusText));
        _postContent?.SetText(content.StatusText);
        
        // Hide elements if not a post
        _actionBar?.SetVisible(content.ItemType != StatusItemType.QuotedPost);
        _avatarContainer?.SetVisible(content.ItemType != StatusItemType.QuotedPost);
        _profilePicture?.SetVisible(content.ItemType == StatusItemType.Post);
        _profilePictureMini?.SetVisible(content.ItemType == StatusItemType.QuotedPost);
        
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
        if (!string.IsNullOrEmpty(content.ProfilePictureUrl))
        {
            _ = TrySetProfilePicture(content.ProfilePictureUrl);
        }

        // Fire and forget setting the embedded content
        if (content.EmbeddedContent != null)
        {
            _ = SetEmbeddedContent(content);
        }
    }

    public StatusItem(StatusItemView content, ILoggerFactory loggerFactory)
        : this(loggerFactory, new Builder("StatusItem.ui"), content)
    {
    }

    private Task SetEmbeddedContent(StatusItemView content)
    {
        // Ignore, if there is no embed content
        if (content.EmbeddedContent == null)
        {
            return Task.CompletedTask;
        }

        _ = content.EmbeddedContent switch
        {
            ViewExternal externalLink => SetExternalLinkContent(externalLink),
            ViewImages images => SetExternalImagesContent(images),
            ViewImage image => SetExternalImagesContent(new ViewImages() { Images = [image] }),
            ViewRecordDef record => SetEmbeddedStatus(record),
            // TODO: Video
            _ => Task.CompletedTask
        };

        return Task.CompletedTask;
    }

    private Task SetEmbeddedStatus(ViewRecordDef record)
    {
        if (_embeddedContentRecordFrame == null || 
            _embeddedRecordDefContainer == null ||
            _externalLinkContainer == null)
        {
            return Task.CompletedTask;
        }

        if (record.Record is ViewRecord viewRecord)
        {
            // Make the containers visible
            _embeddedContentRecordFrame.SetVisible(true);
            _embeddedRecordDefContainer.SetVisible(true);

            var newRecordItem = new StatusItem(
                new StatusItemView(viewRecord),
                _loggerFactory);

            _embeddedRecordDefContainer.Append(newRecordItem);
        }

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
            && _embeddedContentLink == null
            && _embeddedContentFrame == null)
        {
            return;
        }

        _embeddedContentFrame?.SetVisible(true);
        _externalLinkContainer?.SetVisible(true);

        _embeddedContentHeadline?.SetLabel(
            externalContent
                .External.Title
                .Trim()
                .Replace(Environment.NewLine, " ")
                .CutWithEllipsis(maxChars: 200));

        _embeddedContentDescription?.SetLabel(
            externalContent.External.Description
                .Trim()
                .Replace(Environment.NewLine, " ")
                .CutWithEllipsis(maxChars: 200));

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
                    _embeddedContentThumbnail.Paintable = Texture.NewFromBytes(thumbnailBytes);
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
            var bytes = await NetworkFunction.GetDataInBytesAsync(profilePictureUrl);

            if (bytes == null)
            {
                return;
            }

            // Set the profile picture
            var texture = Texture.NewFromBytes(bytes);
            
            if (_profilePicture != null)
            {
                _profilePicture.CustomImage = texture;
            }

            if (_profilePictureMini != null)
            {
                _profilePictureMini.CustomImage = texture;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failure setting profile picture on status item: {Url}", _content.Uri);
        }
    }

    private void ReplyButtonOnClicked(Button sender, EventArgs args)
    {
        throw new NotImplementedException();
    }
    
    private static string GetSubHandle(StatusItemView content)
    {
        // Get the profile handle
        var handle = content.UserHandle;

        // Get the created time
        var createdAt = content.PublishedAt;

        return createdAt != null ? $"{handle} · {createdAt.Value.ToRelativeDate()}" : handle;
    }
    
    private static void HeartButtonOnToggled(ToggleButton sender, EventArgs args)
    {
        SetToggleButtonStyle(sender, "post_love_button_toggled");
    }

    private static void RepostButtonOnToggled(ToggleButton sender, EventArgs args)
    {
        SetToggleButtonStyle(sender, "post_repost_button_toggled");
    }

    private static void SetToggleButtonStyle(ToggleButton sender, string styleClass)
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
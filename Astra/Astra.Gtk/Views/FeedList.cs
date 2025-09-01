using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using Astra.AtProtocol.Common;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
using Astra.Gtk.Views.Providers;
using Gtk;
using Gtk.Internal;
using Microsoft.Extensions.Logging;
using Box = Gtk.Box;
using Builder = Gtk.Builder;
using ListBox = Gtk.ListBox;
using ScrolledWindow = Gtk.ScrolledWindow;

namespace Astra.Gtk.Views;

public class Feed : Box
{
    [Connect("status_list_box")]
    private readonly ListBox? _feedListBox = null;

    [Connect("feed_spinner")]
    private readonly Box? _feedSpinner = null;

    [Connect("feed_scroll")]
    private readonly ScrolledWindow? _feedScroll = null;

    [Connect("banner")]
    private readonly Adw.Banner? _banner = null;

    // Source
    private readonly AtFeed _source;
    
    // Services
    private readonly IToastProvider _toastProvider;
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly INavigationProvider _navigationProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Feed> _logger;
    private readonly Builder _builder;

    // Last cursor
    private string _lastCursor = string.Empty;

    private Feed(
        AtFeed source,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory,
        INavigationProvider navigationProvider,
        IToastProvider toastProvider,
        Builder builder) :
        base(new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        ArgumentNullException.ThrowIfNull(_feedListBox);

        _source = source;
        _logger = loggerFactory.CreateLogger<Feed>();
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        _navigationProvider = navigationProvider;
        _builder = builder;
        _loggerFactory = loggerFactory;
        _toastProvider = toastProvider;

        _feedListBox.OnRowActivated += (sender, args) =>
        {
            if (args.Row is StatusItem statusItem)
            {
                var statusContent = statusItem.GetContent();
                
                _navigationProvider.Go(
                    new StatusItemPage(
                        statusContent.Uri,
                        _userFeedService,
                        _navigationProvider,
                        _toastProvider,
                        _loggerFactory));
            }
        };

        _ = InitializeAt(builder);
    }

    public Feed(
        AtFeed source,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory,
        INavigationProvider navigationProvider,
        IToastProvider toastProvider)
        : this(
            source,
            sessionService,
            userFeedService,
            credentialProvider,
            loggerFactory,
            navigationProvider,
            toastProvider,
            new Builder("Feed.ui"))
    {
    }

    private async Task InitializeAt(Builder builder)
    {
        await FetchPosts();

        var scrollAdjustment = _feedScroll?.Vadjustment;

        if (scrollAdjustment != null)
        {
            scrollAdjustment.OnValueChanged += async (sender, args) =>
            {
                if (!string.IsNullOrEmpty(_lastCursor) &&
                    sender.Value >= sender.Upper - sender.PageSize)
                {
                    _logger.LogDebug("Hit end of scroll, fetch more posts...");
                    await FetchPosts(_lastCursor);
                }
            };
        }
    }

    private async Task FetchPosts(string? cursor = null)
    {
        await Task.Run(async () =>
        {
            try
            {
                FeedResult feedResult;

                if (_source.Type == CommonConstants.FollowingFeedTypeName)
                {
                    feedResult = await _userFeedService.GetFollowingFeed(
                        limit: CommonConstants.FeedFetchLimit,
                        cursor: cursor,
                        token: CancellationToken.None);
                }
                else
                {
                    feedResult = await _userFeedService.GetFeed(
                        feedUri: _source.Value,
                        limit: CommonConstants.FeedFetchLimit,
                        cursor: cursor,
                        token: CancellationToken.None);
                }

                _lastCursor = feedResult.Cursor ?? string.Empty;
                
                var statusUpdates = feedResult
                    .Posts
                    .Select(x => x.Post)
                    .ToList();

                var statusItems = statusUpdates
                    .Select(status => new StatusItem(
                        new StatusItemView(status),
                        StatusItemMode.PostListItem,
                        _loggerFactory,
                        _userFeedService,
                        _navigationProvider,
                        _toastProvider))
                    .ToArray();

                foreach (var statusItem in statusItems)
                {
                    _feedListBox?.Append(statusItem);
                }

                ShowSpinner(false);
                HideBanner();
            }
            catch
            {
                ShowSpinner(false);
                ShowBanner("Failed to fetch feed");
            }
        });
    }

    private void ShowSpinner(bool enabled)
    {
        _feedSpinner?.SetVisible(enabled);
        _feedScroll?.SetVisible(!enabled);
    }

    public void Refresh()
    {
        _feedListBox?.RemoveAll();
        _lastCursor = string.Empty;
        
        ShowSpinner(true);
        ShowBanner("Refreshing...");
        
        _ = FetchPosts();
    }

    private void ShowBanner(string message, int autoHideSec = 30)
    {
        if (_banner == null)
        {
            return;
        }
        
        _banner.Title = message;
        _banner.Revealed = true;
    }

    private void HideBanner()
    {
        if (_banner == null)
        {
            return;
        }

        _banner.Revealed = false;
    }
}
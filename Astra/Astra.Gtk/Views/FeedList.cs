using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
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
    [Connect("status_list_box")] private readonly ListBox? _feedListBox = null;

    [Connect("feed_spinner")] private readonly Box? _feedSpinner = null;

    [Connect("feed_scroll")] private readonly ScrolledWindow? _feedScroll = null;

    // Services
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Feed> _logger;
    private readonly Builder _builder;

    // Last cursor
    private string _lastCursor = string.Empty;

    private Feed(
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory,
        Builder builder) :
        base(new BoxHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);

        _logger = loggerFactory.CreateLogger<Feed>();
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        _builder = builder;
        _loggerFactory = loggerFactory;

        _ = InitializeAt(builder);
    }

    public Feed(
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory)
        : this(
            sessionService,
            userFeedService,
            credentialProvider,
            loggerFactory,
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
            var timelineResult = await _userFeedService.GetUserTimeline(
                limit: 20,
                cursor: cursor,
                token: CancellationToken.None);

            _lastCursor = timelineResult.Cursor ?? string.Empty;

            var statusUpdates = timelineResult
                .Posts
                .Select(x => x.Post)
                .ToList();

            var statusItems = statusUpdates
                .Select(status => new StatusItem(new StatusItemView(status), _loggerFactory))
                .ToArray();

            foreach (var statusItem in statusItems)
            {
                _feedListBox?.Append(statusItem);
            }

            _feedSpinner?.SetVisible(false);
            _feedScroll?.SetVisible(true);
        });
    }
}
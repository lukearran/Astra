using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
using Astra.Gtk.Resources;
using GObject;
using Gtk;
using Microsoft.Extensions.Logging;
using Builder = Gtk.Builder;

namespace Astra.Gtk.Views;

public class FeedNavPage : Adw.NavigationPage
{
    private static FeedNavPage? _instance;

    public static FeedNavPage Instance(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Adw.OverlaySplitView sourceOverlay)
    {
        return _instance ??= new FeedNavPage(
            loggerFactory,
            sessionService,
            userFeedService,
            credentialProvider,
            sourceOverlay);
    }
    
    [Connect("feed_stack")]
    private readonly Adw.ViewStack? _feedStack = null;
    
    [Connect("show_sidebar_button")]
    private readonly ToggleButton? _toggleButton = null;
    
    [Connect("refresh_button")]
    private readonly Button? _refreshButton = null;

    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly ILogger<FeedNavPage> _logger;
    
    private readonly ILoggerFactory _loggerFactory;
    
    private FeedNavPage(
        ILoggerFactory loggerFactory,
        Builder builder,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Adw.OverlaySplitView sourceOverlay) : base(
        new NavigationPageHandle(builder.GetPointer("_root"), false))
    {
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<FeedNavPage>();
        
        builder.Connect(this);
        
        ArgumentNullException.ThrowIfNull(_refreshButton);

        sourceOverlay.BindProperty(
            "show-sidebar",
            _toggleButton ?? throw new NullReferenceException(), 
            "active",
            BindingFlags.Bidirectional);
        
        sourceOverlay.BindProperty(
            "show-sidebar",
            _toggleButton ?? throw new NullReferenceException(), 
            "active",
            BindingFlags.SyncCreate);
        
        _refreshButton.OnClicked += OnRefreshButtonClicked;

        _ = SetDisplayFeeds();
    }

    private FeedNavPage(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Adw.OverlaySplitView sourceOverlay)
        : this(
            loggerFactory,
            new Builder("FeedNavPage.ui"),
            sessionService,
            userFeedService,
            credentialProvider,
            sourceOverlay)
    {
        
    }
    
    private void OnRefreshButtonClicked(Button sender, EventArgs args)
    {
        var currentPage = _feedStack?.VisibleChild as Feed ?? throw new NullReferenceException();
        currentPage.Refresh();
    }

    private async Task SetDisplayFeeds()
    {
        var userFeeds = _sessionService
            .GetCurrentSession()?
            .UserPreferences?
            .Feeds
            .Where(x => x.IsPinned)
            .ToList();
        
        foreach (var userFeed in userFeeds?.Slice(0, userFeeds.Count > 5 ? 5 : userFeeds.Count) ?? [])
        {
            try
            {
                var feedDetail = await _userFeedService.GetFeedDetail(userFeed);

                var feedView = new Feed(
                    userFeed,
                    _sessionService,
                    _userFeedService,
                    _credentialProvider,
                    _loggerFactory);

                _feedStack?.AddTitledWithIcon(
                    child: feedView,
                    name: feedDetail.DisplayName,
                    title: feedDetail.DisplayName,
                    iconName: FeedIconMapper.MapIconName(userFeed.Value));
            }
            catch (Exception ex)
            {
                // TODO: Notify user of error
                _logger.LogError(ex, "Failed to instantiate feed '{FeedName}'", userFeed.Id);
            }
        }
    }
}
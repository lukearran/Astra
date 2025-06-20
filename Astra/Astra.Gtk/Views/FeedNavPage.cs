using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.Gtk.Resources;
using Astra.Gtk.Views.Providers;
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
        Adw.OverlaySplitView sourceOverlay,
        INavigationProvider navigationProvider)
    {
        return _instance ??= new FeedNavPage(
            loggerFactory,
            sessionService,
            userFeedService,
            credentialProvider,
            sourceOverlay,
            navigationProvider);
    }
    
    [Connect("feed_stack")]
    private readonly Adw.ViewStack? _feedStack = null;
    
    [Connect("refresh_button")]
    private readonly Button? _refreshButton = null;
    
    [Connect("toast_overlay")]
    private readonly Adw.ToastOverlay? _toastOverlay = null;
    
    [Connect("show_sidebar_button")]
    private readonly ToggleButton? _toggleButton = null;

    private readonly INavigationProvider _navigationProvider;
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
        INavigationProvider navigationProvider,
        Adw.OverlaySplitView sourceOverlay) : base(
        new NavigationPageHandle(builder.GetPointer("_root"), false))
    {
        builder.Connect(this);
        
        _navigationProvider = navigationProvider;
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        _loggerFactory = loggerFactory;
        _logger = _loggerFactory.CreateLogger<FeedNavPage>();
        
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
        
        ArgumentNullException.ThrowIfNull(_refreshButton);
        
        _refreshButton.OnClicked += OnRefreshButtonClicked;

        _ = SetDisplayFeeds();
    }

    private FeedNavPage(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Adw.OverlaySplitView sourceOverlay,
        INavigationProvider navigationProvider)
        : this(
            loggerFactory,
            new Builder("FeedNavPage.ui"),
            sessionService,
            userFeedService,
            credentialProvider,
            navigationProvider,
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
            string? feedName = null;
            
            try
            {
                var feedDetail = await _userFeedService.GetFeedDetail(userFeed);

                feedName = feedDetail.DisplayName;
                
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
                _logger.LogError(ex, "Failed to instantiate feed '{FeedName}'", userFeed.Id);
                
                ShowToastMessage($"Error loading {feedName ?? "your feed"}");
            }
        }
    }

    private void ShowToastMessage(string message) =>
        _toastOverlay?.AddToast(Adw.Toast.New(message));
}
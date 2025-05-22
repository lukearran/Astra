using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
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
    
    // TODO: This list should be populated from API. For now, display a placeholder following view
    private readonly List<FeedAlgorithmView>  _feedAlgorithms = [
        new FeedAlgorithmView(displayName: "Following", displayIcon: "people-symbolic", algorithm: "todo"),
    ];
    
    private FeedNavPage(
        ILoggerFactory loggerFactory,
        Builder builder,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Adw.OverlaySplitView sourceOverlay) : base(
        new NavigationPageHandle(builder.GetPointer("_root"), false))
    {
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

        // Create stack pages from algorithms
        foreach (var algo in _feedAlgorithms)
        {
            var feed = new Feed(
                sessionService,
                userFeedService,
                credentialProvider,
                loggerFactory);
            
            _feedStack?.AddTitledWithIcon(
                child: feed,
                name: algo.DisplayName,
                title: algo.DisplayName,
                iconName: algo.DisplayIcon);
        }
    }

    public FeedNavPage(
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
}
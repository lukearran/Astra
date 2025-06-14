using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.Gtk.Functions;
using Gdk;
using Gtk;
using Microsoft.Extensions.Logging;

namespace Astra.Gtk.Views;

public class MainWindow : Adw.ApplicationWindow
{
    [Connect("sidebar_menu")]
    private readonly ListBox? _sidebarMenuList = null;
    
    [Connect("split_view")]
    private readonly Adw.OverlaySplitView? _overlaySplitView = null;
    
    [Connect("avatar")]
    private readonly Adw.Avatar? _avatar = null;

    private readonly ISessionService _sessionService;
    
    private readonly IUserFeedService _userFeedService;
    
    private readonly ICredentialProvider _credentialProvider;
    
    private readonly ILogger<MainWindow> _logger;
    
    private readonly ILoggerFactory _loggerFactory;
    
    private readonly List<SidebarMenuItem> _sidebarMenuItems = [
        new("user-home-symbolic", "Home", typeof(FeedNavPage)),
    ];
    
    private MainWindow(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Builder builder,
        string name) : base(new ApplicationWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);

        _loggerFactory = loggerFactory;
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        
        _logger = loggerFactory.CreateLogger<MainWindow>();
        
        _sidebarMenuItems.ForEach(item => _sidebarMenuList?.Append(item));

        if (_sidebarMenuList != null)
        {
            _sidebarMenuList.OnRowSelected += OnSidebarMenuItemSelected;
        }
        
        _sidebarMenuList?.SelectRow(_sidebarMenuItems.First());

        _ = SetProfileView();
    }

    public MainWindow(
        Adw.Application application,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory)
        : this(
            loggerFactory,
            sessionService,
            userFeedService,
            credentialProvider,
            new Builder("MainWindow.ui"), 
            "main_window")
    {
        this.Application = application;
    }
    
    private void OnSidebarMenuItemSelected(ListBox sender, ListBox.RowSelectedSignalArgs args)
    {
        if (args.Row is not SidebarMenuItem sidebarMenuItem)
        {
            return;
        }
        
        _logger.LogDebug("'{SidebarMenuItemName}' sidebar item selected, using {PageType} page type...",
            sidebarMenuItem.Name, sidebarMenuItem.PageViewType);
        
        if (sidebarMenuItem.PageViewType == typeof(FeedNavPage))
        {
            _overlaySplitView!.Content = FeedNavPage.Instance(
                _loggerFactory,
                _sessionService,
                _userFeedService,
                _credentialProvider,
                _overlaySplitView ?? throw new NullReferenceException());
        }
    }

    private async Task SetProfileView()
    {
        var currentSession = _sessionService.GetCurrentSession();

        if (currentSession == null)
            return;
        
        _avatar?.SetText(currentSession?.Profile?.DisplayName ?? string.Empty);
        
        if (!string.IsNullOrEmpty(currentSession?.Profile?.AvatarUrl))
        {
            try
            {
                var avatarBytes = await NetworkFunction.GetDataInBytesAsync(currentSession.Profile.AvatarUrl);

                if (avatarBytes != null && _avatar != null)
                {
                    _avatar.CustomImage = Texture.NewFromBytes(avatarBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load the session avatar: {ThumbnailUrl}", currentSession.Profile.AvatarUrl);
            }
        }
    }
}
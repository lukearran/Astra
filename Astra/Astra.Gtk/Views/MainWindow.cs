using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Gtk;
using Microsoft.Extensions.Logging;

namespace Astra.Gtk.Views;

public class MainWindow : Adw.ApplicationWindow
{
    [Connect("home_page_container")]
    private readonly Box? _homeStackContainer = null;
    
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<MainWindow> _logger;
    private readonly Builder _builder;

    private MainWindow(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider,
        Builder builder,
        string name) : base(new ApplicationWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);

        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        _builder = builder;
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<MainWindow>();
        
        // Set the panel
        _homeStackContainer?.Append(new Feed(
            _sessionService,
            _userFeedService,
            _credentialProvider,
            _loggerFactory));
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
}
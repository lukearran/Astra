using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Providers;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gtk;
using Microsoft.Extensions.Logging;

namespace Astra.Gtk.Views;

public class MainWindow : Adw.ApplicationWindow
{
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

        _ = InitializeAt();
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
    
    private async Task InitializeAt()
    {
        ArgumentNullException.ThrowIfNull(_credentialProvider.Username);
        ArgumentNullException.ThrowIfNull(_credentialProvider.Password);
        
        // TODO The following initialising process is just placeholder until later replaced with an async solution
        var loginSessionResult = await _sessionService.LoginWithPassword(
            identifier: _credentialProvider.Username,
            password: _credentialProvider.Password,
            CancellationToken.None
        );

        var timelineResult = await _userFeedService.GetUserTimeline(limit: 50, CancellationToken.None);

        // Initialize the ListBox
        var listBoxObj = _builder.GetObject("status_list_box");

        if (listBoxObj is not ListBox box)
        {
            throw new Exception("status_list_box not found on window");
        }
            
        // Example list of status updates
        var statusUpdates = timelineResult
            .Posts
            .Select(x => x.Post)
            .ToList();

        // Add status updates to the ListBox
        AddStatusUpdates(statusUpdates, ref box);
    }

    private void AddStatusUpdates(List<PostView> statusUpdates, ref ListBox statusListBox)
    {
        foreach (var row in statusUpdates.Select(status => new StatusItem(status, _loggerFactory)))
        {
            statusListBox.Append(row);
        }
    }
}
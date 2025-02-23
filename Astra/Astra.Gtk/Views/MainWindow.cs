using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Providers;
using FishyFlip.Lexicon.App.Bsky.Feed;
using Gtk;

namespace Astra.Gtk.Views;

public class MainWindow : Adw.ApplicationWindow
{
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly Builder _builder;

    private MainWindow(
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

        _ = InitializeAt();
    }

    public MainWindow(
        Adw.Application application,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider)
        : this(
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
        foreach (var status in statusUpdates)
        {
            var row = new StatusItem(status);
            statusListBox.Append(row);
        }
    }
}
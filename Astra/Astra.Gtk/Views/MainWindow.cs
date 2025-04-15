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

        _builder = builder;
        _logger = loggerFactory.CreateLogger<MainWindow>();
        
        var feed = new Feed(
            sessionService,
            userFeedService,
            credentialProvider,
            loggerFactory);
        
        _homeStackContainer?.Append(feed);
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

    public void Refresh()
    {
        _logger.LogInformation("Refreshing feed");

        var feed = (Feed)_homeStackContainer?.GetFirstChild()!;
        feed.Refresh();
    }
}
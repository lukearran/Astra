using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
using Astra.AtProtocol.Common.Providers;
using FishyFlip.Lexicon.App.Bsky.Feed;
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
    [Connect("feed_spinner")]
    private readonly Box? _feedSpinner = null;
    
    [Connect("feed_scroll")]
    private readonly ScrolledWindow? _feedScroll = null;
    
    // Services
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<Feed> _logger;
    private readonly Builder _builder;
    
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

        Task.Run(() =>
        {
            _ = InitializeAt(builder);
        });
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
        ArgumentNullException.ThrowIfNull(_credentialProvider.Username);
        ArgumentNullException.ThrowIfNull(_credentialProvider.Password);
        
        await _sessionService.LoginWithPassword(
            identifier: _credentialProvider.Username,
            password: _credentialProvider.Password,
            CancellationToken.None
        );

        var timelineResult = await _userFeedService.GetUserTimeline(limit: 20, CancellationToken.None);

        // Initialize the ListBox
        var listBoxObj = builder.GetObject("status_list_box");

        if (listBoxObj is not ListBox box)
        {
            throw new Exception("status_list_box not found on window");
        }
            
        // Example list of status updates
        var statusUpdates = timelineResult
            .Posts
            .Select(x => x.Post)
            // TODO: Allow viewing of replies to posts
            .Where(x => x.PostRecord?.Reply == null)
            .ToList();

        // Add status updates to the ListBox
        AddStatusUpdates(statusUpdates, ref box);
        
        _feedSpinner?.SetVisible(false);
        _feedScroll?.SetVisible(true);
    }
    
    private void AddStatusUpdates(List<PostView> statusUpdates, ref ListBox statusListBox)
    {
        foreach (var row in statusUpdates.Select(status => 
                     new StatusItem(new StatusItemView(status), _loggerFactory)))
        {
            statusListBox.Append(row);
        }
    }
}
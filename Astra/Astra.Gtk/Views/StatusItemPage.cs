using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Models.Views;
using Astra.Gtk.Views.Providers;
using Gtk;
using Microsoft.Extensions.Logging;

namespace Astra.Gtk.Views;

public class StatusItemPage : Adw.NavigationPage
{
    private readonly IUserFeedService _userFeedService;
    private readonly INavigationProvider  _navigationProvider;
    private readonly IToastProvider _toastProvider;
    private readonly ILoggerFactory _loggerFactory;
    
    [Connect("list")]
    private readonly ListBox? _listBox = null;

    [Connect("_view")]
    private readonly Adw.ToolbarView? _toolbarView = null;

    [Connect("feed_spinner")]
    private readonly Box? _spinnerContainer = null;
    
    private StatusItemPage(
        Builder builder,
        IUserFeedService userFeedService,
        INavigationProvider navigationProvider,
        IToastProvider toastProvider,
        ILoggerFactory loggerFactory,
        string postUri) : base(
        new NavigationPageHandle(builder.GetPointer("_root"), 
            false))
    {
        builder.Connect(this);

        _userFeedService = userFeedService;
        _navigationProvider = navigationProvider;
        _loggerFactory = loggerFactory;
        _toastProvider = toastProvider;

        _ = Fetch(postUri);
    }
    
    public StatusItemPage(
        string postUri,
        IUserFeedService userFeedService,
        INavigationProvider navigationProvider,
        IToastProvider toastProvider,
        ILoggerFactory loggerFactory)
        : this(new Builder("StatusItemPage.ui"),
            userFeedService,
            navigationProvider,
            toastProvider,
            loggerFactory,
            postUri)
    {
    }

    private async Task Fetch(string uri)
    {
        var thread = await _userFeedService.GetPostThread(uri);
        
        if (!thread.Success || thread.Post == null)
        {
            _toolbarView!.Content = new ErrorStatusPage(
                title: "Failed to display post",
                description: "Sorry. An error occurred fetching the post. Please try again.");
            return;
        }
        
        // Set the title
        var content = new StatusItemView(thread.Post);
        
        Title = $"{thread.Post.Author.DisplayName}";

        if (!string.IsNullOrEmpty(content.StatusText))
        {
            Title += $": {content.StatusText}";
        }
        
        // Create the parent post
        var statusItemView = new StatusItem(
            content: content,
            mode: StatusItemMode.ThreadParentPost,
            _loggerFactory,
            _userFeedService,
            _navigationProvider,
            _toastProvider);

        _listBox?.Append(statusItemView);
        
        // Create the reply posts
        if (thread.Replies.Count != 0)
        {
            var replyViews = 
                thread.Replies.Select(x => new StatusItemView(x));

            var replyResult = replyViews.Select(x =>
                new StatusItem(
                    content: x,
                    mode: StatusItemMode.ThreadReplyPost,
                    _loggerFactory,
                    _userFeedService,
                    _navigationProvider,
                    _toastProvider));
            
            foreach (var replyItem in replyResult)
            {
                _listBox?.Append(replyItem);
            }
        }
        
        _spinnerContainer?.SetVisible(false);
    }
}
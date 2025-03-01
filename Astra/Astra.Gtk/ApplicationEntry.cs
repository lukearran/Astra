using System.Reflection;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Providers;
using Astra.Gtk.Views;
using Gio;
using Microsoft.Extensions.Logging;
using File = System.IO.File;

namespace Astra.Gtk;

public class ApplicationEntry
{
    private readonly ILogger<ApplicationEntry> _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly Application _application;
    
    public ApplicationEntry(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider)
    {
        _loggerFactory = loggerFactory;
        _logger = loggerFactory.CreateLogger<ApplicationEntry>();
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        
        _application = Adw.Application.New(AppId, ApplicationFlags.DefaultFlags);
        _application.OnActivate += OnActivate;
        _application.OnStartup += OnOnStartup;
        
        var gResourcePath = Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty) + "/app.astra.gtk.gresource";
        
        if (File.Exists(gResourcePath))
        {
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(gResourcePath));
        }
        else
        {
            var prefixes = new List<string> {
                Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
                Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
                "/usr"
            };

            foreach (var prefix in prefixes.Where(prefix => File.Exists(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource")))
            {
                Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource")));
                break;
            }
        }
    }
    
    public void Run(string[] args) => _application.RunWithSynchronizationContext(args);

    private void OnOnStartup(Gio.Application sender, EventArgs args)
    {
        var window = new MainWindow(
            (Adw.Application)sender,
            _sessionService,
            _userFeedService,
            _credentialProvider,
            _loggerFactory);
        
        window.Present();
    }

    private void OnActivate(Application sender, EventArgs args)
    {
        // TODO: Register callbacks for the application
    }
}
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
    private readonly ISessionService _sessionService;
    private readonly IUserFeedService _userFeedService;
    private readonly ICredentialProvider _credentialProvider;
    
    private string[] _args = [];
    private Application _application;
    
    public ApplicationEntry(
        ILogger<ApplicationEntry> logger,
        ISessionService sessionService,
        IUserFeedService userFeedService,
        ICredentialProvider credentialProvider
        )
    {
        _logger = logger;
        _sessionService = sessionService;
        _userFeedService = userFeedService;
        _credentialProvider = credentialProvider;
        
        _application = Adw.Application.New(AppId, ApplicationFlags.DefaultFlags);
        _application.OnActivate += OnActivate;
        _application.OnStartup += OnOnStartup;
        
        // Register resources
        if (File.Exists(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/app.astra.gtk.gresource"))
        {
            //Load file from program directory, required for `dotnet run`
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "/app.astra.gtk.gresource"));
        }
        else
        {
            var prefixes = new List<string> {
                Directory.GetParent(Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName).FullName,
                Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))).FullName,
                "/usr"
            };
            foreach (var prefix in prefixes)
            {
                if (File.Exists(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource"))
                {
                    Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(Path.GetFullPath(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource")));
                    break;
                }
            }
        }
    }
    
    public void Run(string[] args)
    {
        _args = args;

        _application.RunWithSynchronizationContext(args);
    }

    private void OnOnStartup(Gio.Application sender, EventArgs args)
    {
        var window = new MainWindow(
            (Adw.Application)sender,
            _sessionService,
            _userFeedService,
            _credentialProvider);
        
        window.Present();
    }

    private void OnActivate(Application sender, EventArgs args)
    {
        // TODO: Register callbacks for the application
    }
}
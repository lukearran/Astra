using System.Reflection;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
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
        _application.OnStartup += OnStartup;
        _application.OnActivate += OnActivate;

        SetupResources();
    }

    public void Run(string[] args) => _application.RunWithSynchronizationContext(args);

    private void OnStartup(Gio.Application sender, EventArgs args) => StartupHook(sender);
    
    private void OnActivate(Application sender, EventArgs args)
    {
       
    }

    private void StartupHook(Gio.Application sender)
    {
        // If no credentials exist, go through on-boarding
        if (_credentialProvider.GetCredential() is null)
        {
            var onboardWindow = new OnboardWindow(
                (Adw.Application)_application,
                _sessionService,
                _credentialProvider,
                _loggerFactory);

            onboardWindow.OnSuccess += (o, args) => ShowMainWindow();
            
            onboardWindow.Show();
        }
        // Otherwise, go straight to main window
        else
        {
            ShowMainWindow();
        }
    }

    private void ShowMainWindow()
    {
        var mainWindow = new MainWindow(
            (Adw.Application)_application,
            _sessionService,
            _userFeedService,
            _credentialProvider,
            _loggerFactory);
        
        mainWindow.Show();
    }

    private static void SetupResources()
    {
        var gResourcePath =
            Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty) +
            "/app.astra.gtk.gresource";

        if (File.Exists(gResourcePath))
        {
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(gResourcePath));
        }
        else
        {
            var prefixes = new List<string>
            {
                Directory.GetParent(Directory
                    .GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
                    .FullName).FullName,
                Directory.GetParent(Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)))
                    .FullName,
                "/usr"
            };

            foreach (var prefix in prefixes.Where(prefix =>
                         File.Exists(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource")))
            {
                Gio.Functions.ResourcesRegister(
                    Gio.Functions.ResourceLoad(
                        Path.GetFullPath(prefix + "/share/app.astra.gtk/app.astra.gtk.gresource")));
                break;
            }
        }
    }
}
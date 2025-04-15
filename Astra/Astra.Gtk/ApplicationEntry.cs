using System.Reflection;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Models;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Credentials;
using Astra.Gtk.Views;
using Gio;
using Microsoft.Extensions.Logging;
using Application = Gio.Application;
using File = System.IO.File;
using Task = System.Threading.Tasks.Task;

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

    private void OnStartup(Gio.Application sender, EventArgs args)
    {
        _ = StartupHook(sender);

        var refresh = SimpleAction.New("refresh", null);
        refresh.OnActivate += (sender, args) =>
        {
            _mainWindow?.Refresh();
        };
        _application.AddAction(refresh);
    }

    private void OnActivate(Application sender, EventArgs args)
    {
    }

    private async Task<bool> IsCredentialsValid()
    {
        AtCredential? credentialsResult = null;
        AtSessionResult? sessionResult = null;

        try
        {
            _application.Hold();
            
            credentialsResult = await _credentialProvider.GetCredential();

            if (credentialsResult is not null)
            {
                var cancelTokenSource = new CancellationTokenSource();

                cancelTokenSource.CancelAfter(TimeSpan.FromSeconds(5));

                sessionResult = await _sessionService.LoginWithPassword(
                    credentialsResult.Username,
                    credentialsResult.Password,
                    cancelTokenSource.Token);

                _logger.LogInformation(
                    "Credentials existed via Secret Service. Is Valid: {IsValid}",
                    sessionResult.Success);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while validating credentials");
        }
        finally
        {
            _application.Release();
        }

        return credentialsResult != null && (sessionResult?.Success ?? false);
    }

    private async Task StartupHook(Gio.Application sender)
    {
        try
        {
            var isCredentialsValid = await IsCredentialsValid();

            if (!isCredentialsValid)
            {
                var onboardWindow = new OnboardWindow(
                    (Adw.Application)_application,
                    _sessionService,
                    _credentialProvider,
                    _loggerFactory);

                onboardWindow.OnSuccess += (o, args) => ShowMainWindow();
                onboardWindow.Show();
            }
            else
            {
                ShowMainWindow();
            }
        }
        catch (System.Exception ex)
        {
            _logger.LogError(ex, "Error while starting up");
            
            // TODO: Notify user of error
        }
    }

    private MainWindow? _mainWindow;

    private void ShowMainWindow()
    {
        _application.Release();

        _mainWindow = new MainWindow(
            (Adw.Application)_application,
            _sessionService,
            _userFeedService,
            _credentialProvider,
            _loggerFactory);

        _mainWindow.Show();
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
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

    private MainWindow? _mainWindow;

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

        SetupResources(_logger);
    }

    public void Run(string[] args) => _application.RunWithSynchronizationContext(args);

    private void OnStartup(Gio.Application sender, EventArgs args)
    {
        _ = StartupHook(sender);
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

    private static void SetupResources(ILogger<ApplicationEntry> logger)
    {
        var gResourcePath =
            Path.GetFullPath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty) +
            "/io.github.lukearran.astra.gresource";
        
        if (File.Exists(gResourcePath))
        {
            logger.LogInformation("gResourcePath exists at {Path}", gResourcePath);
            
            Gio.Functions.ResourcesRegister(Gio.Functions.ResourceLoad(gResourcePath));
        }
        else
        {
            throw new InvalidOperationException($"No file at expected gResourcePath: {gResourcePath}");
        }
    }
}
using Adw;
using Adw.Internal;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Common.Interfaces;
using Astra.AtProtocol.Common.Models.Credentials;
using Gtk;
using Microsoft.Extensions.Logging;
using Spinner = Gtk.Spinner;

namespace Astra.Gtk.Views;

public class OnboardWindow : Adw.ApplicationWindow
{
    // Widget control
    [Connect("username_entry")] private readonly Adw.EntryRow? _usernameEntry = null;

    [Connect("password_entry")] private readonly Adw.PasswordEntryRow? _passwordEntry = null;

    [Connect("sign_in_button")] private readonly Button? _signInButton = null;
    
    [Connect("sign_in_button_spinner")] private readonly Spinner? _signInButtonSpinner = null;
    
    [Connect("toast_overlay")] private readonly Adw.ToastOverlay? _toastOverlay = null;

    // Injected services
    private readonly ISessionService _sessionService;
    private readonly ICredentialProvider _credentialProvider;
    private readonly ILogger<OnboardWindow> _logger;

    // Events
    public event EventHandler? OnSuccess;

    private OnboardWindow(
        ILoggerFactory loggerFactory,
        ISessionService sessionService,
        ICredentialProvider credentialProvider,
        Builder builder,
        string name) : base(new ApplicationWindowHandle(builder.GetPointer(name), false))
    {
        builder.Connect(this);
        
        _sessionService = sessionService;
        _credentialProvider = credentialProvider;
        _logger = loggerFactory.CreateLogger<OnboardWindow>();
        
        ArgumentNullException.ThrowIfNull(_signInButton);

        _signInButton.OnClicked += SignInButtonOnClicked;
    }

    public OnboardWindow(
        Adw.Application application,
        ISessionService sessionService,
        ICredentialProvider credentialProvider,
        ILoggerFactory loggerFactory)
        : this(
            loggerFactory,
            sessionService,
            credentialProvider,
            new Builder("Onboard.ui"),
            "_root")
    {
        this.Application = application;
    }

    private void SignInButtonOnClicked(Button sender, EventArgs args)
    {
        if (string.IsNullOrEmpty(_usernameEntry?.GetText())
            && string.IsNullOrEmpty(_passwordEntry?.GetText()))
        {
            _logger.LogWarning("Invalid username or password user input.");
            return;
        }

        _ = HandleSignIn(_usernameEntry!.GetText(), _passwordEntry!.GetText());
    }

    private async Task HandleSignIn(string username, string password)
    {
        SetSignInProgressState(true);
        
        var sessionResult = await _sessionService.LoginWithPassword(
            username,
            password,
            CancellationToken.None);

        if (sessionResult.Success is false)
        {
            _logger.LogWarning(
                "Session failed with user provided username and password, aborting setting the credential");

            SetSignInProgressState(false);
            ShowToast("Invalid credentials. Please try again.");
            return;
        }

        var saved = await _credentialProvider.SetCredential(new AtCredential(username, password));

        if (saved is false)
        {
            _logger.LogWarning("Failed to write credentials, aborting setting the credential");
            
            SetSignInProgressState(false);
            ShowToast("Error occured saving credentials, please try again");
            
            return;
        }

        OnSuccess?.Invoke(this, EventArgs.Empty);
        Close();
    }

    private void SetSignInProgressState(bool inProgress)
    {
        _signInButton!.Sensitive = !inProgress;
        _signInButtonSpinner!.SetVisible(inProgress);
    }

    private void ShowToast(string message)
    {
        var toast = new Adw.Toast();
        toast.Title = message;
        toast.Priority = ToastPriority.High;
        
        _toastOverlay?.AddToast(toast);
    }
}
global using static Astra.Gtk.Constants;
using System.Reflection;
using Astra.AtProtocol.Client.Interfaces;
using Astra.AtProtocol.Client.Services;
using Astra.AtProtocol.Common.Providers;
using Astra.Gtk;
using FishyFlip;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

var logger = new SerilogLoggerFactory().CreateLogger(AppId);

Gio.Module.Initialize();

var atProtocolBuilder = new ATProtocolBuilder()
    .EnableAutoRenewSession(true)
    .WithLogger(logger);

var atProtocol = atProtocolBuilder.Build();

var serviceProvider = new ServiceCollection()
    .AddLogging(builder => { builder.AddSerilog(); })
    .AddSingleton(atProtocol)
    .AddSingleton<ISessionService, SessionService>()
    .AddSingleton<IUserFeedService, UserFeedService>()
    .AddSingleton<ICredentialProvider, CredentialProvider>((x) => new CredentialProvider(
        defaultUsername: Environment.GetEnvironmentVariable("at_username"),
        defaultPassword: Environment.GetEnvironmentVariable("at_password")))
    .AddSingleton<ApplicationEntry>()
    .BuildServiceProvider();
    
var app = serviceProvider.GetRequiredService<ApplicationEntry>();

// Debug - Write all embedded resources, only when in debug
var executingAssembly = Assembly.GetExecutingAssembly();
executingAssembly.GetManifestResourceNames()
    .ToList()
    .ForEach(x => logger.LogDebug("Embedded resource is present - {ResourceName}", x));

app.Run(args);
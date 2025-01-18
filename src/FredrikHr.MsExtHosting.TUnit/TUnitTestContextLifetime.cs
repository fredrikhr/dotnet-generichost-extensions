using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.Hosting.TUnit;

public sealed partial class TUnitTestContextLifetime(
    IHostEnvironment environment,
    IHostApplicationLifetime appLifetime,
    ILoggerFactory? loggerFactory = null
    ) : IHostLifetime, IDisposable
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Performance",
        "CA1823: Avoid unused private fields",
        Justification = nameof(LoggerMessageAttribute)
        )]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "CodeQuality",
        "IDE0079: Remove unnecessary suppression"
        )]
    private readonly ILogger _logger = (loggerFactory ?? Microsoft.Extensions
        .Logging.Abstractions.NullLoggerFactory.Instance)
        .CreateLogger("Microsoft.Hosting.Lifetime");
    private CancellationTokenRegistration _applicationStartedRegistration;
    private CancellationTokenRegistration _applicationStoppingRegistration;

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public Task WaitForStartAsync(CancellationToken cancellationToken)
    {
        TestContext context = TestContext.Current ??
            throw new InvalidOperationException("TestContext.Current is null");

        _applicationStartedRegistration = appLifetime.ApplicationStarted
            .Register(ApplicationStartedCallback, this);
        _applicationStoppingRegistration = appLifetime.ApplicationStopping
            .Register(ApplicationStopptingCallback, this);

        context.Events.OnTestEnd += OnTestEnd;
        return Task.CompletedTask;

        static void ApplicationStartedCallback(object? state)
        {
            ((TUnitTestContextLifetime)state!).OnApplicationStarted();
        }

        static void ApplicationStopptingCallback(object? state)
        {
            ((TUnitTestContextLifetime)state!).OnApplicationStopping();
        }
    }

    private void OnApplicationStarted()
    {
        LogHostingEnvironment(environment.EnvironmentName);
        LogContentRootPath(environment.ContentRootPath);
    }

    private void OnApplicationStopping()
    {
        LogApplicationStopping();
    }

    private Task OnTestEnd(object sender, TestContext context)
    {
        appLifetime.StopApplication();
        return Task.CompletedTask;
    }

    [LoggerMessage(Level = LogLevel.Information, Message = $"Hosting environment: {{{nameof(EnvName)}}}", EventId = 1, EventName = nameof(appLifetime.ApplicationStarted))]
    private partial void LogHostingEnvironment(string EnvName);

    [LoggerMessage(Level = LogLevel.Information, Message = $"Content root path: {{{nameof(ContentRootPath)}}}", EventId = 2, EventName = nameof(appLifetime.ApplicationStarted))]
    private partial void LogContentRootPath(string ContentRootPath);

    [LoggerMessage(Level = LogLevel.Information, Message = $"Application is shutting down...", EventId = 99, EventName = nameof(appLifetime.ApplicationStopping))]
    private partial void LogApplicationStopping();

    public void Dispose()
    {
        _applicationStartedRegistration.Dispose();
        _applicationStoppingRegistration.Dispose();
    }
}
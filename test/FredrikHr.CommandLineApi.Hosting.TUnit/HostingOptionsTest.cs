using System.CommandLine;
using System.CommandLine.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.TUnit;
using Microsoft.Extensions.Options;

namespace FredrikHr.CommandLineApi.Hosting.TUnit;

public class HostingOptionsTest
{
    public class HostingCommandLineOptions
    {
        public int IntOption { get; set; }
        public required string StringOption { get; set; }
    }

    public class HostingCommandLineInvocation(
        IOptions<HostingCommandLineOptions> options,
        TaskCompletionSource<HostingCommandLineOptions> tcs
        ) : IHostCommandLineInvocation
    {
        public Task<int> InvokeAsync(CancellationToken cancelToken = default)
        {
            tcs.SetResult(options.Value);
            return Task.FromResult(0);
        }
    }

    [Test]
    public async Task Test1()
    {
        Option<int> intOption = new("--int-option", "-i");
        intOption.Configure((HostingCommandLineOptions options, int value) =>
            options.IntOption = value
        );
        TaskCompletionSource<HostingCommandLineOptions> optionsSource = new();
        RootCommand rootCommand = [intOption];
        rootCommand.UseHost<HostingCommandLineInvocation>(
            Host.CreateDefaultBuilder,
            hostBuilder =>
            {
                hostBuilder.ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddTUnit();
                });
                hostBuilder.ConfigureServices(services =>
                {
                    services.AddSingleton(optionsSource);
                });
            });
        int invocationResult = await rootCommand.Parse(["--int-option", "42"])
            .InvokeAsync().ConfigureAwait(continueOnCapturedContext: false);
        await Assert.That(invocationResult).IsEqualTo(0);
        var optionsInstance = await optionsSource.Task
            .ConfigureAwait(continueOnCapturedContext: false);
        await Assert.That(optionsInstance.IntOption).IsEqualTo(42);
    }
}

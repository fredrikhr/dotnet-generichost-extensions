using System.CommandLine.Hosting;

using Microsoft.Extensions.Options;
using Microsoft.Identity.Client;

internal sealed class MsalAcquireTokenForClientExecution(
    IOptions<IConfidentialClientApplication> msalClientAccessor,
    IEnumerable<IConfigureOptions<AcquireTokenForClientParameterBuilder>> tokenAcquisitionConfigures,
    IEnumerable<IPostConfigureOptions<AcquireTokenForClientParameterBuilder>> tokenAcquisitionPostConfigures
    ) : ICommandLineHostedExecution
{
    private readonly IConfidentialClientApplication _msalClient = msalClientAccessor.Value;

    private void ConfigureTokenAcquisition(AcquireTokenForClientParameterBuilder builder)
    {
        foreach (var configureBuilder in tokenAcquisitionConfigures)
        {
            configureBuilder.Configure(builder);
        }
        foreach (var postConfigureBuilder in tokenAcquisitionPostConfigures)
        {
            postConfigureBuilder.PostConfigure(name: default, builder);
        }

    }

    public Task<int> InvokeAsync(CancellationToken cancelToken = default)
    {
        throw new NotImplementedException();
    }
}
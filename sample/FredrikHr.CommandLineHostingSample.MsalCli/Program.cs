using System.CommandLine;
using System.CommandLine.Hosting;

using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Client;

RootCommand cliRoot = new()
{
    Options =
    {
        new Option<string>("--application-id", [
            "--app-id",
            "--appid",
            "--client-id",
            "--client",
            "--id",
            "-c"
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
            Recursive = true,
        }.Configure<ApplicationOptions, string>((o, value) => o.ClientId = value),
        new Option<string>("--tenant-id", [
            "--tenant-domain-name",
            "--tenant-name",
            "--tenant",
            "-t"
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
            Recursive = true,
        }.Configure<ApplicationOptions, string>((o, value) => o.TenantId = value),
        new Option<AadAuthorityAudience>("--sign-in-audience", [
            "--sign-in-portal",
            "--sign-in",
            "-g"
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
        }.Configure<ApplicationOptions, AadAuthorityAudience>((o, value) => o.AadAuthorityAudience = value),
        new Option<AzureCloudInstance>("--entraid-instance", [
            "--azure-cloud-instance",
            "--azure-instance",
            "--instance",
            "-e",
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
            Recursive = true,
        }.Configure<ApplicationOptions, AzureCloudInstance>((o, value) => o.AzureCloudInstance = value),
        new Option<string>("--entraid-instance-url", [
            "--azure-cloud-instance-url",
            "--azure-instance-url",
            "--instance-url",
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
            Recursive = true,
            Hidden = true,
        }.Configure<ApplicationOptions , string>((o, value) => o.Instance = value),
        new Option<string>("--redirect-uri", [
            "--redirect",
            "-r"
            ])
        {
            Arity = ArgumentArity.ExactlyOne,
            Recursive = true,
        }.Configure<ApplicationOptions , string>((o, value) => o.RedirectUri = value)
    },
    Subcommands =
    {
        new Command("confidential")
        {
            Options =
            {
                new Option<string>("--client-secret", [
                    "--secret",
                    "-p"
                    ])
                {
                    Arity = ArgumentArity.ExactlyOne,
                    Recursive = true,
                }.Configure<ConfidentialClientApplicationOptions, string>((o, value) => o.ClientSecret = value),
            },
        }.UseHostExecution<MsalAcquireTokenForClientExecution>(ConfigureHost),
    },
};
return await cliRoot.Parse(args ?? [])
    .InvokeAsync()
    .ConfigureAwait(continueOnCapturedContext: false);

static void ConfigureHost(HostApplicationBuilder hostBuilder)
{
hostBuilder.Services.AddMsal()
        .UseLogging(enablePiiLogging: hostBuilder.Environment.IsDevelopment())
        .UseHttpClientFactory();
}

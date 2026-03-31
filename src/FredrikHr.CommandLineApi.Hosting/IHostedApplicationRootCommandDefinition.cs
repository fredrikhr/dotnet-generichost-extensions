namespace System.CommandLine.Hosting;

public interface IHostedApplicationRootCommandDefinition
    : IHostedApplicationCommandDefinition
{
    RootCommand RootCommand { get; }

#if NET8_0_OR_GREATER
    [Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1033: Interface methods should be callable by child types",
        Justification = "default implemented interface member"
        )]
    Command IHostedApplicationCommandDefinition.Command => RootCommand;
#endif
}

#if NET8_0_OR_GREATER
    public interface IHostedApplicationRootCommandDefinition<TSelf>
    : IHostedApplicationRootCommandDefinition, IHostedApplicationCommandDefinition<TSelf>
    where TSelf : IHostedApplicationRootCommandDefinition<TSelf>
{ }
#endif

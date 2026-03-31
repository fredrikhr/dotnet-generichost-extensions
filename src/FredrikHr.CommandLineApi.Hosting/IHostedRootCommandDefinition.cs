namespace System.CommandLine.Hosting;

public interface IHostedRootCommandDefinition : IHostedCommandDefinition
{
    RootCommand RootCommand { get; }

#if NET8_0_OR_GREATER
    [Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1033: Interface methods should be callable by child types",
        Justification = "default implemented interface member"
        )]
    Command IHostedCommandDefinition.Command => RootCommand;
#endif
}

#if NET8_0_OR_GREATER
    public interface IHostedRootCommandDefinition<TSelf>
    : IHostedRootCommandDefinition, IHostedCommandDefinition<TSelf>
    where TSelf : IHostedRootCommandDefinition<TSelf>
{ }
#endif
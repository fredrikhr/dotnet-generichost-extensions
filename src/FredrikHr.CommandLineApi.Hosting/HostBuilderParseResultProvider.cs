using Microsoft.Extensions.Hosting;

namespace System.CommandLine.Hosting;

public static class HostBuilderParseResultProvider
{
    public static ParseResult? GetCommandLineParseResult(
        this IHostBuilder hostBuilder
        ) => hostBuilder?.Properties.TryGetValue(
            typeof(ParseResult),
            out object? parseResult
            ) ?? false
        ? parseResult as ParseResult
        : null;

    public static ParseResult? GetCommandLineParseResult(
        this HostBuilderContext context
        ) => context?.Properties.TryGetValue(
            typeof(ParseResult),
            out object? parseResult
            ) ?? false
        ? parseResult as ParseResult
        : null;
}

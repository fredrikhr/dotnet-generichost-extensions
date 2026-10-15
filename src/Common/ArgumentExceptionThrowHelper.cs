#if !NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

internal static class ArgumentExceptionThrowHelper
{
    extension(ArgumentNullException)
    {
        internal static void ThrowIfNull(
            [NotNull] object? argument,
            [CallerArgumentExpression(nameof(argument))]
            string? paramName = null)
        {
            _ = argument ?? throw new ArgumentNullException(paramName);
        }
    }
}
#endif
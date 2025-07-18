// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(Microsoft.Extensions.DependencyInjection)
    )]

[assembly: SuppressMessage(
    "Style",
    "IDE0130: Namespace does not match folder structure",
    Scope = "namespace", Target = "~N:Azure.Core.Diagnostics"
    )]

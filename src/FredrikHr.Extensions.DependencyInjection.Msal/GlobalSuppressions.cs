// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Style",
    "IDE0130: Namespace does not match folder structure",
    Scope = "namespace", Target = "~N:Microsoft.Identity.Client"
    )]
[assembly: SuppressMessage(
    "Style",
    "IDE0130: Namespace does not match folder structure",
    Scope = "namespace", Target = "~N:Microsoft.Identity.Client.Extensions.Msal"
    )]
[assembly: SuppressMessage(
    "Performance",
    "CA1812: Avoid uninstantiated internal classes",
    Justification = nameof(Microsoft.Extensions.DependencyInjection)
    )]
[assembly: SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Target = "~T:Microsoft.Identity.Client.MsalAcquireTokenBuilderOptions",
    Scope = "type",
    Justification = "extension block"
    )]
[assembly: SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Target = "~T:Microsoft.Identity.Client.MsalHttpMessageOptions",
    Scope = "type",
    Justification = "extension block"
    )]
[assembly: SuppressMessage(
    "Naming",
    "CA1708: Identifiers should differ by more than case",
    Target = "~T:Microsoft.Identity.Client.MsalHttpMessageOptions",
    Scope = "type",
    Justification = "extension block"
    )]

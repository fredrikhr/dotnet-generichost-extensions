// This file is used by Code Analysis to maintain SuppressMessage
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given
// a specific target and scoped to a namespace, type, member, etc.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Design",
    "CA1034: Nested types should not be visible",
    Justification = nameof(TUnit)
    )]
[assembly: SuppressMessage(
    "Maintainability",
    "CA1515: Consider making public types internal",
    Justification = nameof(TUnit)
    )]
[assembly: SuppressMessage(
    "Style",
    "IDE0053: Use expression body for lambda expression"
    )]

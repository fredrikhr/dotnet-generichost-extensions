using Microsoft.Extensions.Options;

#if NET8_0_OR_GREATER
using System.Threading;
#else
using Lock = object;
#endif

namespace Azure.Identity;

public class ManagedIdentityRegistry
{
    private readonly Lock _lock = new();

    private ManagedIdentityId _defaultEntry = ManagedIdentityId.SystemAssigned;
    private readonly Dictionary<string, ManagedIdentityId> _entries =
        new(comparer: StringComparer.Ordinal);

    public void SetAll(ManagedIdentityId managedIdentityId)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(managedIdentityId);
#else
        _ = managedIdentityId ?? throw new ArgumentNullException(nameof(managedIdentityId));
#endif
        _defaultEntry = managedIdentityId;
    }

    public void Set(string? name, ManagedIdentityId managedIdentityId)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(managedIdentityId);
#else
        _ = managedIdentityId ?? throw new ArgumentNullException(nameof(managedIdentityId));
#endif
        lock (_lock)
        { _entries[name ?? Options.DefaultName] = managedIdentityId; }
    }

    public ManagedIdentityId Get(string? name)
    {
        name ??= Options.DefaultName;
        lock (_lock)
        {
            if (_entries.TryGetValue(name, out var managedIdentityId))
                return managedIdentityId;
        }
        return _defaultEntry;
    }

    public void Unset(string? name)
    {
        name ??= Options.DefaultName;
        lock (_lock)
        {
            _entries.Remove(name);
        }
    }
}
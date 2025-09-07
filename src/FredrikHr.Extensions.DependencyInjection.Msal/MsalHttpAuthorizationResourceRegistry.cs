namespace Microsoft.Identity.Client;

public sealed class MsalHttpAuthorizationResourceRegistry() : IDisposable
{
    private readonly ReaderWriterLockSlim _lock = new();
    private readonly List<(Uri requestUri, string resource)> _entries =
        new(capacity: 8);

    public Uri[] GetRegisteredUris()
    {
        _lock.EnterReadLock();
        try
        {
            var uris = new Uri[_entries.Count];
            for (int i = 0; i < _entries.Count; i++)
            {
                uris[i] = _entries[i].requestUri;
            }
            return uris;
        }
        finally { _lock.ExitReadLock(); }
    }

    public string? GetResource(Uri? requestUri)
    {
        if (requestUri is null) return null;
        _lock.EnterReadLock();
        try
        {
            foreach ((Uri entryUri, string entryResource) in _entries)
            {
                if (!entryUri.IsBaseOf(requestUri)) continue;
                return entryResource;
            }
        }
        finally { _lock.ExitReadLock(); }

        return null;
    }

    public void AddEntry(Uri uri)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(uri);
#else
        _ = uri ?? throw new ArgumentNullException(nameof(uri));
#endif
        string resource = uri.GetLeftPart(UriPartial.Authority);
        AddEntry(uri, resource);
    }

    public void AddEntry(Uri uri, string resource)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(uri);
#else
        _ = uri ?? throw new ArgumentNullException(nameof(uri));
#endif

        _lock.EnterWriteLock();
        try
        {
            int index = 0;
            foreach ((Uri existingEntryUri, string _) in _entries)
            {
                if (uri == existingEntryUri)
                {
                    _entries[index] = (uri, resource);
                    return;
                }
                if (existingEntryUri.IsBaseOf(uri))
                {
                    _entries.Insert(index, (uri, resource));
                    return;
                }

                index++;
            }
            _entries.Add((uri, resource));
        }
        finally { _lock.ExitWriteLock(); }
    }

    public void RemoveClosestEntry(Uri? requestUri)
    {
        RemoveEntryCore(requestUri, multiple: false);
    }

    public void RemoveMatchingEntries(Uri? requestUri)
    {
        RemoveEntryCore(requestUri, multiple: true);
    }

    private void RemoveEntryCore(Uri? requestUri, bool multiple)
    {
        if (requestUri is null) return;
        _lock.EnterUpgradeableReadLock();
        try
        {
            for (int index = 0; index < _entries.Count; index++)
            {
                Uri entryUri = _entries[index].requestUri;
                if (requestUri == entryUri ||
                    entryUri.IsBaseOf(requestUri))
                {
                    _lock.EnterWriteLock();
                    try
                    {
                        _entries.RemoveAt(index);
                        if (!multiple) return;
                    }
                    finally { _lock.ExitWriteLock(); }
                }
            }
        }
        finally { _lock.ExitUpgradeableReadLock(); }
    }

    public void Dispose()
    {
        _lock.Dispose();
    }
}

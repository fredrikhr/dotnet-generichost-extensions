using System.Collections;

namespace Microsoft.Extensions.Configuration;

public static class ConfigurationBuilderInsertExtensions
{
    private sealed class TemporaryConfigurationSourceList(
        IList<IConfigurationSource> sources, int insertIndex
    ) : IList<IConfigurationSource>
    {
        private readonly List<IConfigurationSource> _newSources = [];

        public IConfigurationSource this[int index]
        {
            get
            {
                return index < insertIndex
                    ? sources[index]
                    : _newSources[index - insertIndex];
            }
            set
            {
                if (index < insertIndex)
                    sources[index] = value;
                else
                    _newSources[index - insertIndex] = value;
            }
        }

        public int Count => insertIndex + _newSources.Count;

        public bool IsReadOnly => false;

        public void Add(IConfigurationSource item) => _newSources.Add(item);

        public void Clear() => _newSources.Clear();

        public bool Contains(IConfigurationSource item)
        {
            int sourcesIdx = sources.IndexOf(item);
            return (sourcesIdx >= 0 && sourcesIdx < insertIndex)
                || _newSources.Contains(item);
        }

        public void CopyTo(IConfigurationSource[] array, int arrayIndex)
        {
            ((ICollection<IConfigurationSource>)_newSources).CopyTo(array, arrayIndex);
        }

        public IEnumerator<IConfigurationSource> GetEnumerator()
        {
            return sources.Take(insertIndex).Concat(_newSources).GetEnumerator();
        }

        public int IndexOf(IConfigurationSource item)
        {
            int sourcesIdx = sources.IndexOf(item);
            if (sourcesIdx >= 0 && sourcesIdx < insertIndex) return sourcesIdx;
            int newIdx = _newSources.IndexOf(item);
            return newIdx >= 0 ? newIdx + insertIndex : newIdx;
        }

        public void Insert(int index, IConfigurationSource item)
        {
            index -= insertIndex;
            _newSources.Insert(index, item);
        }

        public bool Remove(IConfigurationSource item)
        {
            int sourcesIdx = sources.IndexOf(item);
            return sourcesIdx >= 0 && sourcesIdx < insertIndex
                ? throw new InvalidOperationException()
                : _newSources.Remove(item);
        }

        public void RemoveAt(int index)
        {
            index -= insertIndex;
            _newSources.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class TemporaryConfigurationBuilder(IConfigurationBuilder origin, int index) : IConfigurationBuilder
    {
        public IDictionary<string, object> Properties { get; } = origin.Properties;
        public IList<IConfigurationSource> Sources { get; } =
            new TemporaryConfigurationSourceList(origin.Sources, index);

        public IConfigurationBuilder Add(IConfigurationSource source)
        {
            Sources.Add(source);
            return this;
        }

        public IConfigurationRoot Build()
        {
            throw new InvalidOperationException();
        }
    }

    public static IConfigurationBuilder Insert(this IConfigurationBuilder config, int index, Action<IConfigurationBuilder> addAction)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(config);
#else
        _ = config ?? throw new ArgumentNullException(nameof(config));
#endif
        if (addAction is null) return config;

        var tmpBuilder = new TemporaryConfigurationBuilder(config, index);
        addAction(tmpBuilder);
        for (int i = index; i < tmpBuilder.Sources.Count; i++)
        {
            config.Sources.Insert(i, tmpBuilder.Sources[i]);
        }

        return config;
    }
}

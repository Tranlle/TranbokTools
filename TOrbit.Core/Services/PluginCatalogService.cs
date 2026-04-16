using System.Collections.ObjectModel;
using TOrbit.Core.Models;

namespace TOrbit.Core.Services;

public sealed class PluginCatalogService : IPluginCatalogService
{
    private readonly ObservableCollection<PluginEntry> _plugins = [];

    // O(1) lookup index kept in sync with the observable collection.
    private readonly Dictionary<string, PluginEntry> _index = new(StringComparer.OrdinalIgnoreCase);

    public IReadOnlyList<PluginEntry> Plugins => _plugins;
    public IEnumerable<PluginEntry> EnabledPlugins => _plugins.Where(x => x.IsEnabled);

    public void Register(
        IPlugin plugin,
        bool enabledByDefault = true,
        int? sort = null,
        bool isBuiltIn = false,
        bool canDisable = true,
        string? builtInHint = null)
    {
        var id = plugin.Descriptor.Id;

        if (_index.ContainsKey(id))
            throw new InvalidOperationException($"Plugin '{id}' is already registered.");

        if (!id.Contains('.'))
        {
            throw new InvalidOperationException(
                $"Plugin Id \"{id}\" does not follow the reverse-domain naming convention. " +
                "Expected at least two dot-separated segments, e.g. \"torbit.my-plugin\".");
        }

        var entry = new PluginEntry(plugin, enabledByDefault, isBuiltIn, canDisable, builtInHint);
        if (sort.HasValue)
            entry.Sort = sort.Value;

        _plugins.Add(entry);
        _index[id] = entry;
    }

    public PluginEntry? Get(string id) => _index.TryGetValue(id, out var entry) ? entry : null;
}

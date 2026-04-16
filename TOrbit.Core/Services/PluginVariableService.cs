using TOrbit.Core.Models;
using TOrbit.Plugin.Core.Abstractions;
using TOrbit.Plugin.Core.Tools;

namespace TOrbit.Core.Services;

public sealed class PluginVariableService : IPluginVariableService
{
    // Scope prefix used by StorageService during legacy migration.
    internal static string ScopeFor(string pluginId) => $"vars:{pluginId}";

    private readonly IPluginCatalogService _pluginCatalog;
    private readonly IPluginToolRegistry _toolRegistry;
    private readonly IStorageService _storage;

    public PluginVariableService(
        IPluginCatalogService pluginCatalog,
        IPluginToolRegistry toolRegistry,
        IStorageService storage)
    {
        _pluginCatalog = pluginCatalog;
        _toolRegistry = toolRegistry;
        _storage = storage;
    }

    public PluginVariableStore Load()
    {
        var store = new PluginVariableStore();

        // Read all vars:* scopes in one shot and group them in memory for the settings UI.
        const string prefix = "vars:";
        var allRows = _storage.GetAllKvWithMetaByPrefix(prefix);

        foreach (var plugin in _pluginCatalog.Plugins)
        {
            var definitions = plugin.Plugin.Descriptor.VariableDefinitions;
            if (definitions is null || definitions.Count == 0)
                continue;

            var scope = ScopeFor(plugin.Id);
            if (!allRows.TryGetValue(scope, out var rows))
                continue;

            var dbRows = rows.ToDictionary(row => row.Key, StringComparer.OrdinalIgnoreCase);
            foreach (var definition in definitions)
            {
                if (!dbRows.TryGetValue(definition.Key, out var row))
                    continue;

                store.Entries.Add(new PluginVariableEntry
                {
                    PluginId = plugin.Id,
                    Key = definition.Key,
                    Value = row.Value ?? string.Empty,
                    IsEncrypted = row.IsEncrypted
                });
            }
        }

        return store;
    }

    public void Save(PluginVariableStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        // Encrypt first, then batch-write once so persistence happens in a single locked transaction.
        IEnumerable<(string scope, string key, string? value, bool isEncrypted)> batch =
            store.Entries.Select(entry =>
            {
                string? value = entry.Value;
                if (entry.IsEncrypted && !string.IsNullOrEmpty(value))
                {
                    var tool = _toolRegistry.GetTool<IPluginEncryptionTool>(entry.PluginId);
                    if (tool is not null)
                        value = tool.Encrypt(value);
                }

                return (scope: ScopeFor(entry.PluginId), key: entry.Key, value: (string?)value, isEncrypted: entry.IsEncrypted);
            });

        _storage.SetKvBatch(batch);
    }

    public string? GetValue(string pluginId, string key)
    {
        var scope = ScopeFor(pluginId);
        var row = _storage.GetKvWithMeta(scope, key);
        if (row is not null)
        {
            if (row.IsEncrypted && !string.IsNullOrEmpty(row.Value))
            {
                var tool = _toolRegistry.GetTool<IPluginEncryptionTool>(pluginId);
                return tool?.TryDecrypt(row.Value) ?? string.Empty;
            }

            return row.Value;
        }

        return GetDefaultValue(pluginId, key);
    }

    public void InjectAll()
    {
        foreach (var entry in _pluginCatalog.Plugins)
            InjectToPlugin(entry.Plugin);
    }

    public void InjectOne(IPlugin plugin) => InjectToPlugin(plugin);

    private void InjectToPlugin(IPlugin plugin)
    {
        if (plugin is not IPluginVariableReceiver receiver)
            return;

        var definitions = plugin.Descriptor.VariableDefinitions;
        if (definitions is null || definitions.Count == 0)
            return;

        var scope = ScopeFor(plugin.Descriptor.Id);
        var dbValues = _storage.GetAllKvWithMeta(scope)
            .ToDictionary(row => row.Key, row => row.Value, StringComparer.OrdinalIgnoreCase);

        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var definition in definitions)
        {
            // Push the raw persisted value to the plugin. Encrypted values stay encrypted here.
            values[definition.Key] = dbValues.TryGetValue(definition.Key, out var value)
                ? value ?? string.Empty
                : definition.DefaultValue;
        }

        receiver.OnVariablesInjected(values);
    }

    private string? GetDefaultValue(string pluginId, string key)
    {
        var plugin = _pluginCatalog.Plugins.FirstOrDefault(p =>
            string.Equals(p.Id, pluginId, StringComparison.OrdinalIgnoreCase));

        return plugin?.Plugin.Descriptor.VariableDefinitions?
            .FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase))
            ?.DefaultValue;
    }
}

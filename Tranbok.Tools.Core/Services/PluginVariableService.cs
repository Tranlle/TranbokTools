using System.Text.Json;
using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public sealed class PluginVariableService : IPluginVariableService
{
    private const string StoreFileName = "plugin-variables.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static string StoreFilePath => Path.Combine(AppContext.BaseDirectory, StoreFileName);

    private readonly IPluginCatalogService _pluginCatalog;

    public PluginVariableService(IPluginCatalogService pluginCatalog)
    {
        _pluginCatalog = pluginCatalog;
    }

    public PluginVariableStore Load()
    {
        if (!File.Exists(StoreFilePath))
            return new PluginVariableStore();

        try
        {
            var json = File.ReadAllText(StoreFilePath);
            return JsonSerializer.Deserialize<PluginVariableStore>(json, JsonOptions) ?? new PluginVariableStore();
        }
        catch
        {
            return new PluginVariableStore();
        }
    }

    public void Save(PluginVariableStore store)
    {
        ArgumentNullException.ThrowIfNull(store);
        File.WriteAllText(StoreFilePath, JsonSerializer.Serialize(store, JsonOptions));
    }

    public string? GetValue(string pluginId, string key)
    {
        var store = Load();
        var entry = store.Entries.FirstOrDefault(e =>
            string.Equals(e.PluginId, pluginId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Key, key, StringComparison.OrdinalIgnoreCase));

        if (entry is not null)
            return entry.Value;

        // 回退到插件元数据中声明的默认值
        var plugin = _pluginCatalog.Plugins.FirstOrDefault(p =>
            string.Equals(p.Id, pluginId, StringComparison.OrdinalIgnoreCase));
        var definition = plugin?.Plugin.Descriptor.VariableDefinitions?
            .FirstOrDefault(d => string.Equals(d.Key, key, StringComparison.OrdinalIgnoreCase));

        return definition?.DefaultValue;
    }
}

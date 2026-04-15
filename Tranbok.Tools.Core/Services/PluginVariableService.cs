using System.Text.Json;
using Tranbok.Tools.Core.Models;
using Tranbok.Tools.Core.Tools;
using Tranbok.Tools.Plugin.Core.Abstractions;
using Tranbok.Tools.Plugin.Core.Tools;

namespace Tranbok.Tools.Core.Services;

public sealed class PluginVariableService : IPluginVariableService
{
    private const string StoreFileName = "plugin-variables.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static string StoreFilePath =>
        Path.Combine(AppContext.BaseDirectory, StoreFileName);

    private readonly IPluginCatalogService _pluginCatalog;
    private readonly IPluginToolRegistry   _toolRegistry;

    public PluginVariableService(
        IPluginCatalogService pluginCatalog,
        IPluginToolRegistry toolRegistry)
    {
        _pluginCatalog = pluginCatalog;
        _toolRegistry  = toolRegistry;
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    public PluginVariableStore Load()
    {
        if (!File.Exists(StoreFilePath))
            return new PluginVariableStore();

        try
        {
            var json = File.ReadAllText(StoreFilePath);
            return JsonSerializer.Deserialize<PluginVariableStore>(json, JsonOptions)
                   ?? new PluginVariableStore();
        }
        catch
        {
            return new PluginVariableStore();
        }
    }

    // ── Save（明文传入，IsEncrypted=true 的条目由基座 Tool 加密后写盘）─────────

    public void Save(PluginVariableStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        foreach (var entry in store.Entries.Where(e => e.IsEncrypted && !string.IsNullOrEmpty(e.Value)))
        {
            var tool = _toolRegistry.GetTool<IPluginEncryptionTool>(entry.PluginId);
            if (tool is not null)
                entry.Value = tool.Encrypt(entry.Value);
        }

        File.WriteAllText(StoreFilePath, JsonSerializer.Serialize(store, JsonOptions));
    }

    // ── GetValue（用于 Settings UI 显示；自动用 Tool 解密加密字段）────────────

    public string? GetValue(string pluginId, string key)
    {
        var store = Load();
        var entry = store.Entries.FirstOrDefault(e =>
            string.Equals(e.PluginId, pluginId, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Key,      key,      StringComparison.OrdinalIgnoreCase));

        if (entry is not null)
        {
            if (entry.IsEncrypted && !string.IsNullOrEmpty(entry.Value))
            {
                var tool = _toolRegistry.GetTool<IPluginEncryptionTool>(pluginId);
                return tool?.TryDecrypt(entry.Value) ?? string.Empty;
            }
            return entry.Value;
        }

        // 回退到元数据默认值
        return GetDefaultValue(pluginId, key);
    }

    // ── InjectAll（将原始存储值推送给插件，由插件自行解密）──────────────────────
    //
    //  设计原则：基座不解密，只负责把存储的原始值（可能是密文）传给插件。
    //  插件在 OnVariablesInjected 中调用 Context.GetTool<IPluginEncryptionTool>() 自行解密。

    public void InjectAll()
    {
        var store = Load();
        foreach (var entry in _pluginCatalog.Plugins)
            InjectToPlugin(entry.Plugin, store);
    }

    // ── Private ───────────────────────────────────────────────────────────────

    private void InjectToPlugin(IPlugin plugin, PluginVariableStore store)
    {
        if (plugin is not IPluginVariableReceiver receiver)
            return;

        var definitions = plugin.Descriptor.VariableDefinitions;
        if (definitions is null || definitions.Count == 0)
            return;

        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var def in definitions)
        {
            var entry = store.Entries.FirstOrDefault(e =>
                string.Equals(e.PluginId, plugin.Descriptor.Id, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Key,      def.Key,              StringComparison.OrdinalIgnoreCase));

            // 传原始值：有存储值则传存储值（可能是密文），否则传默认值（明文）
            dict[def.Key] = entry is not null ? entry.Value : def.DefaultValue;
        }

        receiver.OnVariablesInjected(dict);
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

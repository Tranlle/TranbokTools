using System.Text.Json;
using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public sealed class KeyMapService : IKeyMapService
{
    private const string StoreFileName = "keymap-bindings.json";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private static string StoreFilePath =>
        Path.Combine(AppContext.BaseDirectory, StoreFileName);

    private readonly List<KeyMapEntry> _entries = [];

    public IReadOnlyList<KeyMapEntry> Entries => _entries;

    public void Register(
        string id,
        string pluginId,
        string pluginName,
        string name,
        string description,
        string defaultKey,
        Action handler)
    {
        var existing = _entries.FirstOrDefault(e => e.Id == id);
        if (existing is not null)
            _entries.Remove(existing);

        _entries.Add(new KeyMapEntry
        {
            Id = id,
            PluginId = pluginId,
            PluginName = pluginName,
            Name = name,
            Description = description,
            DefaultKey = defaultKey,
            Handler = handler,
            IsEnabled = existing?.IsEnabled ?? true,
            CustomKey = existing?.CustomKey
        });
    }

    public bool Dispatch(string keyString)
    {
        if (string.IsNullOrWhiteSpace(keyString))
            return false;

        foreach (var entry in _entries)
        {
            if (!entry.IsEnabled || entry.Handler is null)
                continue;

            if (string.Equals(entry.EffectiveKey, keyString, StringComparison.OrdinalIgnoreCase))
            {
                entry.Handler.Invoke();
                return true;
            }
        }

        return false;
    }

    public void Load()
    {
        if (!File.Exists(StoreFilePath))
            return;

        try
        {
            var json = File.ReadAllText(StoreFilePath);
            var store = JsonSerializer.Deserialize<KeyMapStore>(json, JsonOptions);
            if (store is null)
                return;

            foreach (var stored in store.Entries)
            {
                var entry = _entries.FirstOrDefault(e => e.Id == stored.Id);
                if (entry is null)
                    continue;

                entry.CustomKey = stored.CustomKey;
                entry.IsEnabled = stored.IsEnabled;
            }
        }
        catch
        {
            // 文件损坏时静默回退到默认值
        }
    }

    public void Save()
    {
        var store = new KeyMapStore
        {
            Entries = _entries
                .Select(e => new KeyMapStoreEntry
                {
                    Id = e.Id,
                    CustomKey = e.CustomKey,
                    IsEnabled = e.IsEnabled
                })
                .ToList()
        };

        File.WriteAllText(StoreFilePath, JsonSerializer.Serialize(store, JsonOptions));
    }

    public void Reset(string? id = null)
    {
        if (id is null)
        {
            foreach (var entry in _entries)
            {
                entry.CustomKey = null;
                entry.IsEnabled = true;
            }
        }
        else
        {
            var entry = _entries.FirstOrDefault(e => e.Id == id);
            if (entry is null)
                return;

            entry.CustomKey = null;
            entry.IsEnabled = true;
        }
    }
}

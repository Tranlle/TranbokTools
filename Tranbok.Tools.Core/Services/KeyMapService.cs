using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public sealed class KeyMapService : IKeyMapService
{
    private readonly IStorageService _storage;
    private readonly List<KeyMapEntry> _entries = [];

    // Dispatch 索引：EffectiveKey（不区分大小写）→ 已启用的 KeyMapEntry
    // 在 Register / Load / Reset / Save 之后调用 RebuildIndex() 维护
    private readonly Dictionary<string, KeyMapEntry> _dispatchIndex =
        new(StringComparer.OrdinalIgnoreCase);

    public KeyMapService(IStorageService storage) => _storage = storage;

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
            Id          = id,
            PluginId    = pluginId,
            PluginName  = pluginName,
            Name        = name,
            Description = description,
            DefaultKey  = defaultKey,
            Handler     = handler,
            IsEnabled   = existing?.IsEnabled ?? true,
            CustomKey   = existing?.CustomKey
        });

        RebuildIndex();
    }

    public bool Dispatch(string keyString)
    {
        if (string.IsNullOrWhiteSpace(keyString))
            return false;

        if (!_dispatchIndex.TryGetValue(keyString, out var entry))
            return false;

        entry.Handler?.Invoke();
        return true;
    }

    public void Load()
    {
        var stored = _storage.LoadKeyMapBindings();
        foreach (var row in stored)
        {
            var entry = _entries.FirstOrDefault(e => e.Id == row.Id);
            if (entry is null) continue;
            entry.CustomKey = row.CustomKey;
            entry.IsEnabled = row.IsEnabled;
        }
        RebuildIndex();
    }

    public void Save()
    {
        _storage.SaveKeyMapBindings(_entries.Select(e => new KeyMapStoreEntry
        {
            Id        = e.Id,
            CustomKey = e.CustomKey,
            IsEnabled = e.IsEnabled
        }));
        // Save 可能由 UI 在修改 CustomKey 后调用，索引需同步
        RebuildIndex();
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
            if (entry is null) return;
            entry.CustomKey = null;
            entry.IsEnabled = true;
        }
        RebuildIndex();
    }

    private void RebuildIndex()
    {
        _dispatchIndex.Clear();
        foreach (var entry in _entries)
        {
            if (!entry.IsEnabled || entry.Handler is null) continue;
            // 同一按键有多个绑定时，先注册的优先（与原线性扫描行为一致）
            _dispatchIndex.TryAdd(entry.EffectiveKey, entry);
        }
    }
}

namespace TOrbit.Core.Models;

public sealed class KeyMapEntry
{
    public string Id { get; init; } = string.Empty;
    public string PluginId { get; init; } = string.Empty;
    public string PluginName { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public string DefaultKey { get; init; } = string.Empty;
    public string? CustomKey { get; set; }
    public bool IsEnabled { get; set; } = true;

    /// <summary>不参与序列化，由注册方提供。</summary>
    public Action? Handler { get; init; }

    public string EffectiveKey => string.IsNullOrWhiteSpace(CustomKey) ? DefaultKey : CustomKey;
}

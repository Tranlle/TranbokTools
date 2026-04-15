namespace TOrbit.Core.Models;

public sealed class PluginVariableEntry
{
    public string PluginId { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    /// <summary>Value 字段是否已加密存储（AES-256-GCM Base64）。</summary>
    public bool IsEncrypted { get; set; }
}

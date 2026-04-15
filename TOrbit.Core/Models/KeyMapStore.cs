namespace TOrbit.Core.Models;

public sealed class KeyMapStore
{
    public List<KeyMapStoreEntry> Entries { get; set; } = [];
}

public sealed class KeyMapStoreEntry
{
    public string Id { get; set; } = string.Empty;
    public string? CustomKey { get; set; }
    public bool IsEnabled { get; set; } = true;
}

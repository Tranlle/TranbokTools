using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public interface IPluginCatalogService
{
    IReadOnlyList<PluginEntry> Plugins { get; }
    IEnumerable<PluginEntry> EnabledPlugins { get; }
    void Register(
        IPlugin plugin,
        bool enabledByDefault = true,
        int? sort = null,
        bool isBuiltIn = false,
        bool canDisable = true,
        string? builtInHint = null);
    PluginEntry? Get(string id);
}

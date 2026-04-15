using TOrbit.Plugin.Core.Enums;

namespace TOrbit.Plugin.Core;

public sealed record PluginManifest(
    PluginDescriptor Descriptor,
    string BaseDirectory,
    IReadOnlyCollection<PluginDependency> Dependencies,
    IReadOnlyDictionary<string, string?> Metadata)
{
    public string Id => Descriptor.Id;
    public string Version => Descriptor.Version;
    public PluginIsolationMode IsolationMode => Descriptor.IsolationMode;
}

namespace TOrbit.Plugin.Core;

public sealed record PluginDiscoveryOptions(
    string RootDirectory,
    string SearchPattern = "*",
    bool Recursive = true);

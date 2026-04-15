namespace TOrbit.Core.Models;

public sealed record LoadedPluginDescriptor(
    PluginEntry Entry,
    string AssemblyPath,
    string PluginDirectory);

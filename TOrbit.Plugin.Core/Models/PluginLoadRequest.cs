using TOrbit.Plugin.Core.Enums;

namespace TOrbit.Plugin.Core;

public sealed record PluginLoadRequest(
    PluginManifest Manifest,
    PluginLoadMode? LoadMode = null,
    bool AutoStart = true);

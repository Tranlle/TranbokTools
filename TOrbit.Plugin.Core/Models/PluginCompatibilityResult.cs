using TOrbit.Plugin.Core.Enums;

namespace TOrbit.Plugin.Core;

public sealed record PluginCompatibilityResult(
    PluginCompatibilityStatus Status,
    string? Message = null,
    IReadOnlyCollection<string>? Details = null);

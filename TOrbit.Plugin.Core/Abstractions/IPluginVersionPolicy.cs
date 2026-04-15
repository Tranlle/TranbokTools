namespace TOrbit.Plugin.Core.Abstractions;

public interface IPluginVersionPolicy
{
    PluginCompatibilityResult Evaluate(PluginManifest manifest, HostEnvironmentInfo hostEnvironment);
}

namespace TOrbit.Plugin.Core.Abstractions;

public interface IPluginDependencyResolver
{
    Task<PluginDependencyGraph> ResolveAsync(IEnumerable<PluginManifest> manifests, CancellationToken cancellationToken = default);
}

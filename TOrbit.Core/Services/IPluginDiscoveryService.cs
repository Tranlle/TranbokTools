using TOrbit.Core.Models;

namespace TOrbit.Core.Services;

public interface IPluginDiscoveryService
{
    ValueTask<PluginDiscoveryResult> LoadAsync(string pluginsDirectory, CancellationToken cancellationToken = default);
}

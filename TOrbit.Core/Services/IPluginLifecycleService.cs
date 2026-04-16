namespace TOrbit.Core.Services;

public interface IPluginLifecycleService
{
    Task StopAsync(string pluginId, CancellationToken ct = default);

    Task StartAsync(string pluginId, CancellationToken ct = default);

    Task RestartAsync(string pluginId, CancellationToken ct = default);
}

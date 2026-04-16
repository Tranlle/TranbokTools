using TOrbit.Core.Models;
using TOrbit.Plugin.Core.Base;
using TOrbit.Plugin.Core.Enums;

namespace TOrbit.Core.Services;

public sealed class PluginLifecycleService : IPluginLifecycleService
{
    private readonly IPluginCatalogService _catalog;

    public PluginLifecycleService(IPluginCatalogService catalog)
        => _catalog = catalog;

    public async Task StopAsync(string pluginId, CancellationToken ct = default)
    {
        var entry = GetRequiredEntry(pluginId);
        if (!entry.CanDisable)
            return;

        try
        {
            await entry.Plugin.StopAsync(ct);
            entry.LastError = null;
        }
        catch (Exception ex)
        {
            entry.LastError = ex;
            SetFaulted(entry);
        }
        finally
        {
            entry.NotifyStateChanged();
        }
    }

    public async Task StartAsync(string pluginId, CancellationToken ct = default)
    {
        var entry = GetRequiredEntry(pluginId);

        try
        {
            await entry.Plugin.StartAsync(ct);
            entry.LastError = null;
        }
        catch (Exception ex)
        {
            entry.LastError = ex;
            SetFaulted(entry);
        }
        finally
        {
            entry.NotifyStateChanged();
        }
    }

    public async Task RestartAsync(string pluginId, CancellationToken ct = default)
    {
        var entry = GetRequiredEntry(pluginId);
        if (!entry.CanDisable)
            return;

        try
        {
            await entry.Plugin.StopAsync(ct);
            await entry.Plugin.StartAsync(ct);
            entry.LastError = null;
        }
        catch (Exception ex)
        {
            entry.LastError = ex;
            SetFaulted(entry);
        }
        finally
        {
            entry.NotifyStateChanged();
        }
    }

    private PluginEntry GetRequiredEntry(string pluginId)
        => _catalog.Get(pluginId) ?? throw new InvalidOperationException($"Plugin '{pluginId}' not found.");

    private static void SetFaulted(PluginEntry entry)
    {
        if (entry.Plugin is BasePlugin basePlugin)
            basePlugin.SetState(PluginState.Faulted);
    }
}

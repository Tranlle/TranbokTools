using Microsoft.Extensions.DependencyInjection;
using Tranbok.Tools.Core.Services;
using Tranbok.Tools.Core.Tools;
using Tranbok.Tools.Plugin.Core.Tools;

namespace Tranbok.Tools.Core.DependencyInjection;

public static class ToolHostCoreServiceCollectionExtensions
{
    public static IServiceCollection AddToolHostCore(this IServiceCollection services)
    {
        services.AddSingleton<IAppShellService, AppShellService>();
        services.AddSingleton<IAppPreferencesService, AppPreferencesService>();
        services.AddSingleton<IPluginCatalogService, PluginCatalogService>();
        services.AddSingleton<IPluginDiscoveryService, PluginDiscoveryService>();
        services.AddSingleton<IPluginVariableService, PluginVariableService>();
        services.AddSingleton<IPluginToolRegistry, PluginToolRegistry>();
        services.AddSingleton<IKeyMapService, KeyMapService>();
        return services;
    }
}

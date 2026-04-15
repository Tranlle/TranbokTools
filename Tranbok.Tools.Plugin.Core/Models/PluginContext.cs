using Tranbok.Tools.Plugin.Core.Enums;
using Tranbok.Tools.Plugin.Core.Tools;

namespace Tranbok.Tools.Plugin.Core;

public sealed record PluginContext(
    string PluginId,
    string BaseDirectory,
    IServiceProvider? Services,
    HostEnvironmentInfo HostEnvironment,
    PluginIsolationMode IsolationMode,
    IReadOnlyDictionary<string, object?> Properties)
{
    /// <summary>
    /// 从宿主工具注册中心获取当前插件的工具实例。
    /// 工具按插件 ID 隔离（如 <see cref="IPluginEncryptionTool"/> 的密钥独立存储）。
    /// 若工具类型不支持或注册中心不可用，返回 null。
    /// </summary>
    public T? GetTool<T>() where T : class, IPluginTool
    {
        // 使用 BCL 的 IServiceProvider.GetService(Type)，避免在 Plugin.Core 引入 DI 包
        var registry = Services?.GetService(typeof(IPluginToolRegistry)) as IPluginToolRegistry;
        return registry?.GetTool<T>(PluginId);
    }
}

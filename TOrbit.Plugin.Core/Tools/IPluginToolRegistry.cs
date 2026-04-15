namespace TOrbit.Plugin.Core.Tools;

/// <summary>
/// 宿主工具注册中心。按插件 ID 分发工具实例（单插件单实例，懒创建）。
/// 在 DI 中注册为 Singleton，由 <see cref="PluginContext.GetTool{T}"/> 内部使用。
/// </summary>
public interface IPluginToolRegistry
{
    /// <summary>
    /// 获取指定插件的工具实例。若工具类型不支持则返回 null。
    /// </summary>
    T? GetTool<T>(string pluginId) where T : class, IPluginTool;
}

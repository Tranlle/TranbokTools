namespace TOrbit.Plugin.Core.Tools;

/// <summary>
/// 基座工具能力的标记接口。
/// 所有宿主提供给插件的工具（如加密、日志、网络）均实现此接口，
/// 插件通过 <see cref="PluginContext.GetTool{T}"/> 按类型取用。
/// </summary>
public interface IPluginTool { }

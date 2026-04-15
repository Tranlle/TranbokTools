using System.Collections.Concurrent;
using Tranbok.Tools.Plugin.Core.Tools;

namespace Tranbok.Tools.Core.Tools;

/// <summary>
/// 宿主工具注册中心实现（Singleton）。
/// 当前支持的工具：<see cref="IPluginEncryptionTool"/>（按插件 ID 懒创建，各自独立密钥）。
/// </summary>
public sealed class PluginToolRegistry : IPluginToolRegistry
{
    // 每个 pluginId 对应一个独立的 AesGcmPluginEncryptionTool 实例
    private readonly ConcurrentDictionary<string, AesGcmPluginEncryptionTool> _encryptionTools
        = new(StringComparer.OrdinalIgnoreCase);

    public T? GetTool<T>(string pluginId) where T : class, IPluginTool
    {
        if (typeof(T) == typeof(IPluginEncryptionTool))
        {
            var tool = _encryptionTools.GetOrAdd(
                pluginId,
                id => new AesGcmPluginEncryptionTool(id));
            return tool as T;
        }

        // 未来可在此扩展更多 Tool 类型
        return null;
    }
}

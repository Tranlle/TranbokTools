using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public interface IPluginVariableService
{
    PluginVariableStore Load();
    void Save(PluginVariableStore store);

    /// <summary>
    /// 获取插件变量的解密后的值。优先返回用户已保存的值（自动解密加密字段），
    /// 若不存在则回退到插件元数据中声明的默认值。
    /// </summary>
    string? GetValue(string pluginId, string key);

    /// <summary>
    /// 将所有已注册插件的变量解析后（含解密）推送给实现了
    /// <see cref="Tranbok.Tools.Plugin.Core.Abstractions.IPluginVariableReceiver"/> 的插件。
    /// </summary>
    void InjectAll();
}

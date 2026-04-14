using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

public interface IPluginVariableService
{
    PluginVariableStore Load();
    void Save(PluginVariableStore store);

    /// <summary>
    /// 获取插件变量的值。优先返回用户已保存的值，若不存在则回退到插件元数据中声明的默认值。
    /// </summary>
    string? GetValue(string pluginId, string key);
}

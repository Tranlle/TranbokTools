namespace TOrbit.Plugin.Core.Abstractions;

/// <summary>
/// 可由插件实现，接收宿主注入的变量值。
/// 宿主在插件注册完成及每次保存变量后，会调用 <see cref="OnVariablesInjected"/>。
/// 传入的 values 中已完成解密，缺失的键宿主已使用元数据默认值补全。
/// </summary>
public interface IPluginVariableReceiver
{
    void OnVariablesInjected(IReadOnlyDictionary<string, string> values);
}

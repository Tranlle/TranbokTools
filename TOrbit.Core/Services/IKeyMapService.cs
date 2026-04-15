using TOrbit.Core.Models;

namespace TOrbit.Core.Services;

public interface IKeyMapService
{
    IReadOnlyList<KeyMapEntry> Entries { get; }

    /// <summary>
    /// 注册一条快捷键动作。重复注册同一 Id 时覆盖旧值。
    /// </summary>
    void Register(
        string id,
        string pluginId,
        string pluginName,
        string name,
        string description,
        string defaultKey,
        Action handler);

    /// <summary>
    /// 从宿主 KeyDown 事件中分发键字符串（如 "Ctrl+K"）。
    /// 返回 true 表示已由某个快捷键处理，调用方应将事件标记为已处理。
    /// </summary>
    bool Dispatch(string keyString);

    /// <summary>从文件加载用户覆盖（应用启动后、快捷键注册完成后调用）。</summary>
    void Load();

    /// <summary>将用户覆盖保存到文件。</summary>
    void Save();

    /// <summary>重置快捷键绑定到默认值。传 null 表示重置全部。</summary>
    void Reset(string? id = null);
}

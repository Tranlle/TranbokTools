using Tranbok.Tools.Core.Models;

namespace Tranbok.Tools.Core.Services;

/// <summary>
/// 基座内部存储服务（SQLite）。插件不直接访问此接口，
/// 而是通过 <see cref="Tranbok.Tools.Plugin.Core.Tools.IPluginStorageTool"/> 使用。
/// <para>
/// 作用域规范：
/// <list type="bullet">
///   <item><description>插件运行时 KV：scope = <c>"kv:{pluginId}"</c></description></item>
///   <item><description>插件配置变量：scope = <c>"vars:{pluginId}"</c></description></item>
///   <item><description>应用全局偏好：scope = <c>"tranbok.app"</c></description></item>
/// </list>
/// </para>
/// </summary>
public interface IStorageService
{
    // ── 通用 KV（同步，供启动路径使用）──────────────────────────────────────

    string? GetKv(string scope, string key);
    void SetKv(string scope, string key, string? value);
    void DeleteKv(string scope, string key);
    IReadOnlyDictionary<string, string?> GetAllKv(string scope);

    // ── 带加密标记的 KV（供 PluginVariableService 使用）─────────────────────

    void SetKvWithMeta(string scope, string key, string? value, bool isEncrypted);

    /// <summary>
    /// 在单个事务内批量写入多条带加密标记的 KV，比逐条调用 <see cref="SetKvWithMeta"/> 效率更高。
    /// </summary>
    void SetKvBatch(IEnumerable<(string scope, string key, string? value, bool isEncrypted)> entries);

    KvEntry? GetKvWithMeta(string scope, string key);
    IReadOnlyList<KvEntry> GetAllKvWithMeta(string scope);

    /// <summary>
    /// 批量读取所有 scope 以 <paramref name="scopePrefix"/> 开头的行，按 scope 分组返回。
    /// 用于 Load() 一次查询替代 N 次 per-plugin 查询。
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyList<KvEntry>> GetAllKvWithMetaByPrefix(string scopePrefix);

    // ── KeyMap（宿主专用）────────────────────────────────────────────────────

    IReadOnlyList<KeyMapStoreEntry> LoadKeyMapBindings();
    void SaveKeyMapBindings(IEnumerable<KeyMapStoreEntry> entries);

    // ── 异步 KV（供 IPluginStorageTool 包装层使用）──────────────────────────

    Task<string?> GetAsync(string scope, string key);
    Task SetAsync(string scope, string key, string? value);
    Task DeleteAsync(string scope, string key);
    Task<IReadOnlyDictionary<string, string?>> GetAllAsync(string scope);
}

/// <summary>带加密标记的 KV 行。</summary>
public sealed record KvEntry(string Key, string? Value, bool IsEncrypted);

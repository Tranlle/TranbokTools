namespace TOrbit.Plugin.Core.Tools;

/// <summary>
/// 宿主提供的加密工具。每个插件获得一个独立实例，内部密钥按插件 ID 隔离管理。
/// 插件无需关心密钥细节，直接调用 Encrypt / TryDecrypt 即可。
/// </summary>
public interface IPluginEncryptionTool : IPluginTool
{
    /// <summary>加密明文，返回 Base64 密文。</summary>
    string Encrypt(string plaintext);

    /// <summary>解密 Base64 密文，失败时返回 null。</summary>
    string? TryDecrypt(string ciphertext);
}

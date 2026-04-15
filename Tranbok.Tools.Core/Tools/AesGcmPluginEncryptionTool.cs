using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using Tranbok.Tools.Plugin.Core.Tools;

namespace Tranbok.Tools.Core.Tools;

/// <summary>
/// AES-256-GCM 实现的 <see cref="IPluginEncryptionTool"/>。
/// 每个插件独立拥有一份密钥文件，存于 %APPDATA%/TranbokTools/&lt;pluginId&gt;/.key。
/// Windows 上额外使用 DPAPI 二次保护密钥文件。
/// </summary>
internal sealed class AesGcmPluginEncryptionTool : IPluginEncryptionTool
{
    private const int KeySize   = 32; // AES-256
    private const int NonceSize = 12; // GCM 标准
    private const int TagSize   = 16;
    private const int KeyVersion = 1;

    private readonly byte[] _key;

    internal AesGcmPluginEncryptionTool(string pluginId)
    {
        _key = LoadOrCreateKey(pluginId);
    }

    // ── 公开接口 ──────────────────────────────────────────────────────────────

    public string Encrypt(string plaintext)
    {
        var data   = Encoding.UTF8.GetBytes(plaintext);
        var nonce  = RandomNumberGenerator.GetBytes(NonceSize);
        var cipher = new byte[data.Length];
        var tag    = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, data, cipher, tag);

        var result = new byte[NonceSize + TagSize + cipher.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, NonceSize);
        cipher.CopyTo(result, NonceSize + TagSize);

        return Convert.ToBase64String(result);
    }

    public string? TryDecrypt(string ciphertext)
    {
        try
        {
            var data      = Convert.FromBase64String(ciphertext);
            if (data.Length < NonceSize + TagSize) return null;

            var nonce     = data[..NonceSize];
            var tag       = data[NonceSize..(NonceSize + TagSize)];
            var encrypted = data[(NonceSize + TagSize)..];
            var plain     = new byte[encrypted.Length];

            using var aes = new AesGcm(_key, TagSize);
            aes.Decrypt(nonce, encrypted, tag, plain);
            return Encoding.UTF8.GetString(plain);
        }
        catch
        {
            return null;
        }
    }

    // ── 密钥管理 ─────────────────────────────────────────────────────────────

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static byte[] LoadOrCreateKey(string pluginId)
    {
        var path = KeyPath(pluginId);

        if (File.Exists(path))
        {
            var raw = File.ReadAllBytes(path);

#if WINDOWS
            try { raw = ProtectedData.Unprotect(raw, null, DataProtectionScope.CurrentUser); }
            catch { /* 未受 DPAPI 保护或非 Windows，忽略 */ }
#endif
            // 带版本前缀格式
            if (raw.Length == KeySize + 4 && BitConverter.ToInt32(raw, 0) == KeyVersion)
                return raw[4..];

            // 旧格式：裸 32 字节
            if (raw.Length == KeySize)
                return raw;

            // 格式未知，重新生成
        }

        var key = RandomNumberGenerator.GetBytes(KeySize);
        PersistKey(path, key);
        return key;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static void PersistKey(string path, byte[] key)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        var versioned = new byte[4 + key.Length];
        BitConverter.GetBytes(KeyVersion).CopyTo(versioned, 0);
        key.CopyTo(versioned, 4);

#if WINDOWS
        try
        {
            var prot = ProtectedData.Protect(versioned, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(path, prot);
            return;
        }
        catch { /* fallthrough */ }
#endif
        File.WriteAllBytes(path, versioned);
        TrySetUnixPermissions(path);
    }

    private static void TrySetUnixPermissions(string path)
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "/bin/chmod",
                Arguments       = $"600 \"{path}\"",
                UseShellExecute = false,
                CreateNoWindow  = true
            });
        }
        catch { /* 忽略 */ }
    }

    private static string KeyPath(string pluginId) =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "TranbokTools",
            pluginId,
            ".key");
}

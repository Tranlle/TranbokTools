using TOrbit.Core.Models;

namespace TOrbit.Core.Services;

public sealed class AppPreferencesService : IAppPreferencesService
{
    // 作用域常量供 StorageService 迁移时引用
    internal const string StorageScope = "torbit.app";
    internal const string KeyFontOption = "fontOptionKey";
    internal const string KeyTheme = "themeKey";
    internal const string KeyPalette = "paletteKey";
    internal const string KeyCloseButtonBehavior = "closeButtonBehavior";

    private readonly IStorageService _storage;

    public AppPreferencesService(IStorageService storage) => _storage = storage;

    public AppPreferences Load() => new()
    {
        FontOptionKey = _storage.GetKv(StorageScope, KeyFontOption) ?? "system",
        ThemeKey = _storage.GetKv(StorageScope, KeyTheme) ?? "Dark",
        PaletteKey = _storage.GetKv(StorageScope, KeyPalette) ?? "tranbok-dark",
        CloseButtonBehavior = ParseCloseButtonBehavior(_storage.GetKv(StorageScope, KeyCloseButtonBehavior))
    };

    public void Save(AppPreferences preferences)
    {
        ArgumentNullException.ThrowIfNull(preferences);
        _storage.SetKv(StorageScope, KeyFontOption, preferences.FontOptionKey);
        _storage.SetKv(StorageScope, KeyTheme, preferences.ThemeKey);
        _storage.SetKv(StorageScope, KeyPalette, preferences.PaletteKey);
        _storage.SetKv(StorageScope, KeyCloseButtonBehavior, preferences.CloseButtonBehavior.ToString());
    }

    private static CloseButtonBehavior ParseCloseButtonBehavior(string? value)
        => Enum.TryParse<CloseButtonBehavior>(value, true, out var behavior)
            ? behavior
            : CloseButtonBehavior.MinimizeToTray;
}

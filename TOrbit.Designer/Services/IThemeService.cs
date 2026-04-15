using Avalonia.Media;
using Avalonia.Styling;
using TOrbit.Designer.Models;

namespace TOrbit.Designer.Services;

public interface IThemeService
{
    ThemeVariant CurrentTheme { get; }
    string CurrentPaletteKey { get; }
    string CurrentFontOptionKey { get; }

    IReadOnlyList<ThemePalette> GetAvailablePalettes();

    void SetTheme(ThemeVariant themeVariant);
    void SetTheme(string themeName);
    void SetPalette(string paletteKey);
    void SetFontOption(string fontOptionKey);
    FontFamily ResolveFontFamily(string fontOptionKey);
    void ApplyTheme(string paletteKey);
}
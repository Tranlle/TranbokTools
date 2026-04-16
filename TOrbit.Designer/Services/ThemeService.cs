using System.Reflection;
using Avalonia;
using Avalonia.Media;
using Avalonia.Styling;
using TOrbit.Designer.Models;

namespace TOrbit.Designer.Services;

public sealed class ThemeService : IThemeService
{
    private readonly ThemePaletteRegistry _registry;
    private string _currentPaletteKey;
    private string _currentFontOptionKey = "system";

    public ThemeService(ThemePaletteRegistry registry)
    {
        _registry = registry;
        _currentPaletteKey = registry.GetAll().FirstOrDefault()?.Key ?? "tranbok-dark";
    }

    public ThemeVariant CurrentTheme => Application.Current?.RequestedThemeVariant ?? ThemeVariant.Default;
    public string CurrentPaletteKey => _currentPaletteKey;
    public string CurrentFontOptionKey => _currentFontOptionKey;

    public IReadOnlyList<ThemePalette> GetAvailablePalettes() => _registry.GetAll();

    public void SetTheme(ThemeVariant themeVariant)
    {
        var palette = ResolvePaletteForTheme(themeVariant);
        if (palette is null)
            return;

        ApplyPalette(palette);
    }

    public void SetTheme(string themeName)
    {
        var themeVariant = themeName.Trim().ToLowerInvariant() switch
        {
            "light" => ThemeVariant.Light,
            "dark" => ThemeVariant.Dark,
            _ => ThemeVariant.Default
        };

        SetTheme(themeVariant);
    }

    public void SetPalette(string paletteKey)
    {
        var palette = _registry.Find(paletteKey) ?? _registry.GetAll().FirstOrDefault();
        if (palette is null)
            return;

        _currentPaletteKey = palette.Key;
        ApplyPalette(palette);
    }

    public void SetFontOption(string fontOptionKey)
    {
        _currentFontOptionKey = string.IsNullOrWhiteSpace(fontOptionKey) ? "system" : fontOptionKey.Trim().ToLowerInvariant();

        if (Application.Current is null)
            return;

        var fontFamily = ResolveFontFamily(_currentFontOptionKey);
        //Application.Current.Resources["TranbokAppFontFamily"] = fontFamily;
        Application.Current.Resources["TOrbitAppFontFamily"] = fontFamily;
    }

    public FontFamily ResolveFontFamily(string fontOptionKey)
    {
        var normalized = string.IsNullOrWhiteSpace(fontOptionKey) ? "system" : fontOptionKey.Trim().ToLowerInvariant();

        var familyName = normalized switch
        {
            "inter" => "Inter",
            "segoe-ui" => "Segoe UI",
            "microsoft-yahei-ui" => "Microsoft YaHei UI",
            "arial" => "Arial",
            "bahnschrift" => "Bahnschrift",
            _ when OperatingSystem.IsWindows() => "Segoe UI, Inter",
            _ when OperatingSystem.IsMacOS() => ".AppleSystemUIFont, SF Pro Text, Helvetica Neue, Inter",
            _ => "Noto Sans, DejaVu Sans, Ubuntu, Inter"
        };

        return new FontFamily(familyName);
    }

    public void ApplyTheme(string paletteKey)
    {
        SetPalette(paletteKey);
    }

    private ThemePalette? ResolvePaletteForTheme(ThemeVariant themeVariant)
    {
        var current = _registry.Find(_currentPaletteKey) ?? _registry.GetAll().FirstOrDefault();
        if (current is null)
            return null;

        var targetSuffix = themeVariant == ThemeVariant.Light ? "-light" : "-dark";
        var oppositeSuffix = themeVariant == ThemeVariant.Light ? "-dark" : "-light";

        if (_currentPaletteKey.EndsWith(oppositeSuffix, StringComparison.OrdinalIgnoreCase))
        {
            var familyKey = _currentPaletteKey[..^oppositeSuffix.Length] + targetSuffix;
            var matched = _registry.Find(familyKey);
            if (matched is not null)
                return matched;
        }

        if (current.BaseVariant == themeVariant)
            return current;

        return new ThemePalette
        {
            Key = current.Key,
            Label = current.Label,
            Description = current.Description,
            Source = current.Source,
            IsBuiltIn = current.IsBuiltIn,
            BaseVariant = themeVariant,
            AccentBrush = current.AccentBrush,
            AccentForegroundBrush = current.AccentForegroundBrush,
            AccentSubtleBrush = current.AccentSubtleBrush,
            AccentSubtleForegroundBrush = current.AccentSubtleForegroundBrush,
            BackgroundBrush = current.BackgroundBrush,
            SurfaceBrush = current.SurfaceBrush,
            SurfaceElevatedBrush = current.SurfaceElevatedBrush,
            BorderBrush = current.BorderBrush,
            TextPrimaryBrush = current.TextPrimaryBrush,
            TextSecondaryBrush = current.TextSecondaryBrush,
            TextMutedBrush = current.TextMutedBrush,
            BadgeSuccessBackgroundBrush = current.BadgeSuccessBackgroundBrush,
            BadgeSuccessForegroundBrush = current.BadgeSuccessForegroundBrush,
            BadgeWarningBackgroundBrush = current.BadgeWarningBackgroundBrush,
            BadgeWarningForegroundBrush = current.BadgeWarningForegroundBrush,
            BadgeDangerBackgroundBrush = current.BadgeDangerBackgroundBrush,
            BadgeDangerForegroundBrush = current.BadgeDangerForegroundBrush
        };
    }

    private static void ApplyBrush(string key, string color)
    {
        if (Application.Current is null)
            return;

        Application.Current.Resources[key] = new SolidColorBrush(Color.Parse(color));
    }

    private void ApplyPalette(ThemePalette palette)
    {
        if (Application.Current is null)
            return;

        Application.Current.RequestedThemeVariant = palette.BaseVariant;
        _currentPaletteKey = palette.Key;

        //ApplyBrush("TranbokAccentBrush", palette.AccentBrush);
        ApplyBrush("TOrbitAccentBrush", palette.AccentBrush);
        //ApplyBrush("TranbokAccentForegroundBrush", palette.AccentForegroundBrush);
        ApplyBrush("TOrbitAccentForegroundBrush", palette.AccentForegroundBrush);
        //ApplyBrush("TranbokAccentSubtleBrush", palette.AccentSubtleBrush);
        ApplyBrush("TOrbitAccentSubtleBrush", palette.AccentSubtleBrush);
        //ApplyBrush("TranbokAccentSubtleForegroundBrush", palette.AccentSubtleForegroundBrush);
        ApplyBrush("TOrbitAccentSubtleForegroundBrush", palette.AccentSubtleForegroundBrush);
        //ApplyBrush("TranbokBackgroundBrush", palette.BackgroundBrush);
        ApplyBrush("TOrbitBackgroundBrush", palette.BackgroundBrush);
        //ApplyBrush("TranbokSurfaceBrush", palette.SurfaceBrush);
        ApplyBrush("TOrbitSurfaceBrush", palette.SurfaceBrush);
        //ApplyBrush("TranbokSurfaceElevatedBrush", palette.SurfaceElevatedBrush);
        ApplyBrush("TOrbitSurfaceElevatedBrush", palette.SurfaceElevatedBrush);
        ApplyBrush("TOrbitSurfaceHoverBrush", palette.SurfaceHoverBrush);
        //ApplyBrush("TranbokBorderBrush", palette.BorderBrush);
        ApplyBrush("TOrbitBorderBrush", palette.BorderBrush);
        ApplyBrush("TOrbitBorderHoverBrush", palette.BorderHoverBrush);
        //ApplyBrush("TranbokTextPrimaryBrush", palette.TextPrimaryBrush);
        ApplyBrush("TOrbitTextPrimaryBrush", palette.TextPrimaryBrush);
        //ApplyBrush("TranbokTextSecondaryBrush", palette.TextSecondaryBrush);
        ApplyBrush("TOrbitTextSecondaryBrush", palette.TextSecondaryBrush);
        //ApplyBrush("TranbokTextMutedBrush", palette.TextMutedBrush);
        ApplyBrush("TOrbitTextMutedBrush", palette.TextMutedBrush);
        //ApplyBrush("TranbokBadgeSuccessBackgroundBrush", palette.BadgeSuccessBackgroundBrush);
        ApplyBrush("TOrbitBadgeSuccessBackgroundBrush", palette.BadgeSuccessBackgroundBrush);
        //ApplyBrush("TranbokBadgeSuccessForegroundBrush", palette.BadgeSuccessForegroundBrush);
        ApplyBrush("TOrbitBadgeSuccessForegroundBrush", palette.BadgeSuccessForegroundBrush);
        //ApplyBrush("TranbokBadgeWarningBackgroundBrush", palette.BadgeWarningBackgroundBrush);
        ApplyBrush("TOrbitBadgeWarningBackgroundBrush", palette.BadgeWarningBackgroundBrush);
        //ApplyBrush("TranbokBadgeWarningForegroundBrush", palette.BadgeWarningForegroundBrush);
        ApplyBrush("TOrbitBadgeWarningForegroundBrush", palette.BadgeWarningForegroundBrush);
        //ApplyBrush("TranbokBadgeDangerBackgroundBrush", palette.BadgeDangerBackgroundBrush);
        ApplyBrush("TOrbitBadgeDangerBackgroundBrush", palette.BadgeDangerBackgroundBrush);
        //ApplyBrush("TranbokBadgeDangerForegroundBrush", palette.BadgeDangerForegroundBrush);
        ApplyBrush("TOrbitBadgeDangerForegroundBrush", palette.BadgeDangerForegroundBrush);
        ApplyBrush("ComboBoxDropDownBackground", palette.SurfaceBrush);
        ApplyBrush("ComboBoxDropDownBorderBrush", palette.BorderBrush);
        ApplyBrush("ComboBoxItemBackground", palette.SurfaceBrush);
        ApplyBrush("ComboBoxItemBackgroundPointerOver", palette.SurfaceElevatedBrush);
        ApplyBrush("ComboBoxItemBackgroundSelected", palette.SurfaceElevatedBrush);
        ApplyBrush("ComboBoxItemBackgroundSelectedPointerOver", palette.SurfaceElevatedBrush);
        ApplyBrush("ComboBoxItemForeground", palette.TextPrimaryBrush);
        ApplyBrush("ComboBoxItemForegroundPointerOver", palette.TextPrimaryBrush);
        ApplyBrush("ComboBoxItemForegroundSelected", palette.TextPrimaryBrush);
        ApplyBrush("ComboBoxItemForegroundSelectedPointerOver", palette.TextPrimaryBrush);
        ApplyBrush("ComboBoxItemBorderBrushSelected", palette.BorderBrush);
        ApplyBrush("ComboBoxItemBorderBrushSelectedPointerOver", palette.BorderBrush);

        ApplyMaterialTheme(palette);
    }

    private static void ApplyMaterialTheme(ThemePalette palette)
    {
        if (Application.Current is null)
            return;

        var materialTheme = Application.Current.Styles.FirstOrDefault(style =>
            style.GetType().FullName?.Contains("MaterialTheme", StringComparison.Ordinal) == true);

        if (materialTheme is null)
            return;

        TrySetBaseTheme(materialTheme, palette.BaseVariant);
        TrySetThemeColor(materialTheme, "SetPrimaryColor", palette.AccentBrush);
        TrySetThemeColor(materialTheme, "SetSecondaryColor", palette.AccentBrush);
    }

    private static void TrySetBaseTheme(object materialTheme, ThemeVariant variant)
    {
        var currentThemeProperty = materialTheme.GetType().GetProperty("CurrentTheme", BindingFlags.Public | BindingFlags.Instance);
        var currentTheme = currentThemeProperty?.GetValue(materialTheme);
        if (currentTheme is null)
            return;

        var setBaseThemeMethod = currentTheme.GetType().GetMethod("SetBaseTheme", BindingFlags.Public | BindingFlags.Instance);
        if (setBaseThemeMethod is null)
            return;

        var argumentType = setBaseThemeMethod.GetParameters().FirstOrDefault()?.ParameterType;
        if (argumentType is null)
            return;

        var assembly = currentTheme.GetType().Assembly;
        object? argument = null;

        if (argumentType.IsEnum)
        {
            var name = variant == ThemeVariant.Light ? "Light" : "Dark";
            argument = Enum.Parse(argumentType, name, true);
        }
        else
        {
            var themeType = assembly.GetType("Material.Styles.Themes.Theme");
            var propertyName = variant == ThemeVariant.Light ? "Light" : "Dark";
            argument = themeType?.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Static)?.GetValue(null);
        }

        if (argument is not null)
            setBaseThemeMethod.Invoke(currentTheme, [argument]);

        if (currentThemeProperty?.CanWrite == true)
            currentThemeProperty.SetValue(materialTheme, currentTheme);
    }

    private static void TrySetThemeColor(object materialTheme, string methodName, string colorText)
    {
        var currentThemeProperty = materialTheme.GetType().GetProperty("CurrentTheme", BindingFlags.Public | BindingFlags.Instance);
        var currentTheme = currentThemeProperty?.GetValue(materialTheme);
        if (currentTheme is null)
            return;

        var method = currentTheme.GetType().GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance, [typeof(Color)]);
        if (method is null)
            return;

        method.Invoke(currentTheme, [Color.Parse(colorText)]);

        if (currentThemeProperty?.CanWrite == true)
            currentThemeProperty.SetValue(materialTheme, currentTheme);
    }
}

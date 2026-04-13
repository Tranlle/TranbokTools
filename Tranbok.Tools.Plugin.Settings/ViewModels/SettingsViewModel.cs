using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using Tranbok.Tools.Core.Services;
using Tranbok.Tools.Designer.Models;
using Tranbok.Tools.Designer.Services;

namespace Tranbok.Tools.Plugin.Settings.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;

    [ObservableProperty]
    private string appName = "Tranbok.Tools";

    [ObservableProperty]
    private string theme = "Dark";

    [ObservableProperty]
    private DesignerOptionItem? selectedThemeOption;

    [ObservableProperty]
    private DesignerOptionItem? selectedPaletteOption;

    [ObservableProperty]
    private DesignerOptionItem? selectedAdvancedPaletteOption;

    [ObservableProperty]
    private bool showAdvancedThemeSettings;

    [ObservableProperty]
    private string customThemeDirectory = Path.Combine(AppContext.BaseDirectory, "themes");

    [ObservableProperty]
    private string workspaceRoot = AppContext.BaseDirectory;

    [ObservableProperty]
    private bool useWorkspaceForMigrations = true;

    [ObservableProperty]
    private string statusMessage = "设置已就绪";

    public ObservableCollection<DesignerOptionItem> ThemeOptions { get; } =
    [
        new DesignerOptionItem { Key = "Dark", Label = "深色模式", Description = "适合低亮环境" },
        new DesignerOptionItem { Key = "Light", Label = "浅色模式", Description = "适合明亮环境" }
    ];

    public ObservableCollection<DesignerOptionItem> PaletteOptions { get; } = [];
    public ObservableCollection<DesignerOptionItem> AdvancedPaletteOptions { get; } = [];

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand ResetCommand { get; }

    public SettingsViewModel(IAppShellService shellService, IThemeService themeService)
    {
        _themeService = themeService;

        appName = shellService.AppName;
        workspaceRoot = shellService.WorkspaceRoot;
        theme = themeService.CurrentTheme == ThemeVariant.Light ? "Light" : "Dark";
        SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Key == theme) ?? ThemeOptions.FirstOrDefault();

        foreach (var palette in themeService.GetAvailablePalettes())
        {
            var option = new DesignerOptionItem
            {
                Key = palette.Key,
                Label = palette.Label,
                Description = palette.Description,
                Value = palette
            };

            if (palette.IsBuiltIn)
            {
                PaletteOptions.Add(option);
            }
            else
            {
                AdvancedPaletteOptions.Add(option);
            }
        }

        SelectedPaletteOption = PaletteOptions.FirstOrDefault(option => option.Key == themeService.CurrentPaletteKey) ?? PaletteOptions.FirstOrDefault();
        SelectedAdvancedPaletteOption = AdvancedPaletteOptions.FirstOrDefault(option => option.Key == themeService.CurrentPaletteKey);

        SaveCommand = new RelayCommand(() =>
        {
            Theme = SelectedThemeOption?.Key ?? Theme;
            var paletteKey = ShowAdvancedThemeSettings
                ? SelectedAdvancedPaletteOption?.Key ?? SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey
                : SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey;
            _themeService.SetPalette(paletteKey);
            _themeService.SetTheme(Theme);
            StatusMessage = $"设置已保存，当前方案：{(ShowAdvancedThemeSettings ? SelectedAdvancedPaletteOption?.Label : SelectedPaletteOption?.Label) ?? "默认"}";
        });

        ResetCommand = new RelayCommand(() =>
        {
            AppName = shellService.AppName;
            Theme = "Dark";
            SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Key == Theme);
            SelectedPaletteOption = PaletteOptions.FirstOrDefault(option => option.Key == "tranbok-dark") ?? PaletteOptions.FirstOrDefault();
            SelectedAdvancedPaletteOption = null;
            ShowAdvancedThemeSettings = false;
            WorkspaceRoot = shellService.WorkspaceRoot;
            UseWorkspaceForMigrations = true;
            _themeService.SetPalette(SelectedPaletteOption?.Key ?? "tranbok-dark");
            _themeService.SetTheme(Theme);
            StatusMessage = "设置已重置";
        });
    }
}

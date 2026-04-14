using Avalonia.Styling;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using Tranbok.Tools.Core.Models;
using Tranbok.Tools.Core.Services;
using Tranbok.Tools.Designer.Models;
using Tranbok.Tools.Designer.Services;

namespace Tranbok.Tools.Plugin.Settings.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IAppPreferencesService _preferencesService;
    private readonly IPluginCatalogService _pluginCatalog;
    private readonly IPluginVariableService _variableService;

    // ── 主题设置 ──────────────────────────────────────────────────────────────

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
    private DesignerOptionItem? selectedFontOption;

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

    public ObservableCollection<DesignerOptionItem> FontOptions { get; } = [];
    public ObservableCollection<DesignerOptionItem> PaletteOptions { get; } = [];
    public ObservableCollection<DesignerOptionItem> AdvancedPaletteOptions { get; } = [];

    public bool IsInterFontWarningVisible => SelectedFontOption?.Key == "inter";
    public string FontWarningMessage => "Inter 在当前 Avalonia 版本下存在已知 TextBox 光标重合风险；若你看到输入框末尾光标压到最后一个字符，建议切回\u201c系统推荐\u201d。";

    // ── 插件变量管理 ──────────────────────────────────────────────────────────

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasPluginVariables))]
    [NotifyPropertyChangedFor(nameof(PluginVariableSummary))]
    private ObservableCollection<PluginVariableItemViewModel> pluginVariableItems = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasAddFormKeyHints))]
    private ObservableCollection<DesignerOptionItem> addFormKeyHints = [];

    [ObservableProperty]
    private bool showAddVariableForm;

    [ObservableProperty]
    private DesignerOptionItem? addFormSelectedPlugin;

    [ObservableProperty]
    private string addFormKey = string.Empty;

    [ObservableProperty]
    private string addFormValue = string.Empty;

    public ObservableCollection<DesignerOptionItem> PluginOptions { get; } = [];

    public bool HasPluginVariables => PluginVariableItems.Count > 0;
    public bool HasAddFormKeyHints => AddFormKeyHints.Count > 0;

    public string PluginVariableSummary => PluginVariableItems.Count == 0
        ? "尚未配置任何插件变量"
        : $"共 {PluginVariableItems.Count} 条变量，跨 {PluginVariableItems.Select(x => x.PluginId).Distinct().Count()} 个插件";

    // ── 命令 ──────────────────────────────────────────────────────────────────

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand ResetCommand { get; }
    public IRelayCommand ShowAddVariableFormCommand { get; }
    public IRelayCommand AddVariableCommand { get; }
    public IRelayCommand CancelAddVariableCommand { get; }
    public IRelayCommand<DesignerOptionItem> FillKeyFromHintCommand { get; }

    public SettingsViewModel(
        IAppShellService shellService,
        IThemeService themeService,
        IAppPreferencesService preferencesService,
        IPluginCatalogService pluginCatalog,
        IPluginVariableService variableService)
    {
        _themeService = themeService;
        _preferencesService = preferencesService;
        _pluginCatalog = pluginCatalog;
        _variableService = variableService;

        var preferences = _preferencesService.Load();

        InitializeFontOptions();
        InitializePluginOptions();
        LoadPluginVariables();

        appName = shellService.AppName;
        workspaceRoot = shellService.WorkspaceRoot;
        theme = themeService.CurrentTheme == ThemeVariant.Light ? "Light" : "Dark";
        SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Key == theme) ?? ThemeOptions.FirstOrDefault();
        SelectedFontOption = FontOptions.FirstOrDefault(option => option.Key == preferences.FontOptionKey)
            ?? FontOptions.FirstOrDefault(option => option.Key == themeService.CurrentFontOptionKey)
            ?? FontOptions.FirstOrDefault();

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
                PaletteOptions.Add(option);
            else
                AdvancedPaletteOptions.Add(option);
        }

        SelectedPaletteOption = PaletteOptions.FirstOrDefault(option => option.Key == themeService.CurrentPaletteKey) ?? PaletteOptions.FirstOrDefault();
        SelectedAdvancedPaletteOption = AdvancedPaletteOptions.FirstOrDefault(option => option.Key == themeService.CurrentPaletteKey);

        SaveCommand = new RelayCommand(() =>
        {
            Theme = SelectedThemeOption?.Key ?? Theme;
            var paletteKey = ShowAdvancedThemeSettings
                ? SelectedAdvancedPaletteOption?.Key ?? SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey
                : SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey;
            var fontOptionKey = SelectedFontOption?.Key ?? "system";

            _themeService.SetPalette(paletteKey);
            _themeService.SetTheme(Theme);
            _themeService.SetFontOption(fontOptionKey);
            _preferencesService.Save(new AppPreferences { FontOptionKey = fontOptionKey });
            SavePluginVariables();

            var fontLabel = SelectedFontOption?.Label ?? "系统推荐";
            StatusMessage = $"设置已保存，当前方案：{(ShowAdvancedThemeSettings ? SelectedAdvancedPaletteOption?.Label : SelectedPaletteOption?.Label) ?? "默认"} · 字体：{fontLabel}";
        });

        ResetCommand = new RelayCommand(() =>
        {
            AppName = shellService.AppName;
            Theme = "Dark";
            SelectedThemeOption = ThemeOptions.FirstOrDefault(option => option.Key == Theme);
            SelectedPaletteOption = PaletteOptions.FirstOrDefault(option => option.Key == "tranbok-dark") ?? PaletteOptions.FirstOrDefault();
            SelectedAdvancedPaletteOption = null;
            SelectedFontOption = FontOptions.FirstOrDefault(option => option.Key == "system") ?? FontOptions.FirstOrDefault();
            ShowAdvancedThemeSettings = false;
            WorkspaceRoot = shellService.WorkspaceRoot;
            UseWorkspaceForMigrations = true;
            _themeService.SetPalette(SelectedPaletteOption?.Key ?? "tranbok-dark");
            _themeService.SetTheme(Theme);
            _themeService.SetFontOption("system");
            _preferencesService.Save(new AppPreferences { FontOptionKey = "system" });
            StatusMessage = "设置已重置";
        });

        ShowAddVariableFormCommand = new RelayCommand(() =>
        {
            AddFormSelectedPlugin = PluginOptions.FirstOrDefault();
            AddFormKey = string.Empty;
            AddFormValue = string.Empty;
            ShowAddVariableForm = true;
        });

        AddVariableCommand = new RelayCommand(() =>
        {
            var pluginId = AddFormSelectedPlugin?.Key ?? string.Empty;
            var key = AddFormKey.Trim();
            var value = AddFormValue;

            if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(key))
                return;

            // 若该 pluginId + key 已存在则更新而非重复添加
            var existing = PluginVariableItems.FirstOrDefault(x =>
                x.PluginId == pluginId && x.Key == key);

            if (existing is not null)
            {
                existing.Value = value;
            }
            else
            {
                var pluginName = AddFormSelectedPlugin?.Label ?? pluginId;
                var hints = AddFormKeyHints;
                var hint = hints.FirstOrDefault(h => h.Key == key);
                var description = hint?.Description ?? string.Empty;
                var defaultValue = hint?.Value as string ?? string.Empty;

                PluginVariableItems.Add(new PluginVariableItemViewModel(
                    pluginId: pluginId,
                    pluginName: pluginName,
                    key: key,
                    value: value,
                    defaultValue: defaultValue,
                    description: description,
                    isFromMetadata: hint is not null,
                    onDelete: RemovePluginVariable));
            }

            OnPropertyChanged(nameof(HasPluginVariables));
            OnPropertyChanged(nameof(PluginVariableSummary));

            ShowAddVariableForm = false;
            AddFormKey = string.Empty;
            AddFormValue = string.Empty;
        });

        CancelAddVariableCommand = new RelayCommand(() =>
        {
            ShowAddVariableForm = false;
            AddFormKey = string.Empty;
            AddFormValue = string.Empty;
        });

        FillKeyFromHintCommand = new RelayCommand<DesignerOptionItem>(hint =>
        {
            if (hint is null) return;
            AddFormKey = hint.Key;
            AddFormValue = hint.Value as string ?? string.Empty;
        });
    }

    partial void OnSelectedFontOptionChanged(DesignerOptionItem? value)
    {
        OnPropertyChanged(nameof(IsInterFontWarningVisible));
        OnPropertyChanged(nameof(FontWarningMessage));
    }

    partial void OnAddFormSelectedPluginChanged(DesignerOptionItem? value)
    {
        AddFormKeyHints.Clear();

        if (value is null)
            return;

        var entry = _pluginCatalog.Plugins.FirstOrDefault(p => p.Id == value.Key);
        var definitions = entry?.Plugin.Descriptor.VariableDefinitions;
        if (definitions is null)
            return;

        foreach (var def in definitions)
        {
            AddFormKeyHints.Add(new DesignerOptionItem
            {
                Key = def.Key,
                Label = string.IsNullOrWhiteSpace(def.DisplayName) ? def.Key : def.DisplayName,
                Description = def.Description,
                Value = def.DefaultValue
            });
        }

        OnPropertyChanged(nameof(HasAddFormKeyHints));
    }

    private void RemovePluginVariable(PluginVariableItemViewModel item)
    {
        PluginVariableItems.Remove(item);
        OnPropertyChanged(nameof(HasPluginVariables));
        OnPropertyChanged(nameof(PluginVariableSummary));
    }

    private void LoadPluginVariables()
    {
        PluginVariableItems.Clear();
        var store = _variableService.Load();

        foreach (var entry in store.Entries)
        {
            var plugin = _pluginCatalog.Plugins.FirstOrDefault(p => p.Id == entry.PluginId);
            var pluginName = plugin?.Name ?? entry.PluginId;
            var def = plugin?.Plugin.Descriptor.VariableDefinitions?
                .FirstOrDefault(d => d.Key == entry.Key);

            PluginVariableItems.Add(new PluginVariableItemViewModel(
                pluginId: entry.PluginId,
                pluginName: pluginName,
                key: entry.Key,
                value: entry.Value,
                defaultValue: def?.DefaultValue ?? string.Empty,
                description: def?.Description ?? string.Empty,
                isFromMetadata: def is not null,
                onDelete: RemovePluginVariable));
        }

        OnPropertyChanged(nameof(HasPluginVariables));
        OnPropertyChanged(nameof(PluginVariableSummary));
    }

    private void SavePluginVariables()
    {
        var store = new PluginVariableStore
        {
            Entries = PluginVariableItems
                .Select(item => new PluginVariableEntry
                {
                    PluginId = item.PluginId,
                    Key = item.Key,
                    Value = item.Value
                })
                .ToList()
        };
        _variableService.Save(store);
    }

    private void InitializePluginOptions()
    {
        PluginOptions.Clear();
        foreach (var plugin in _pluginCatalog.Plugins.OrderBy(p => p.Name))
        {
            PluginOptions.Add(new DesignerOptionItem
            {
                Key = plugin.Id,
                Label = plugin.Name,
                Description = plugin.Description
            });
        }
    }

    private void InitializeFontOptions()
    {
        FontOptions.Clear();
        FontOptions.Add(new DesignerOptionItem
        {
            Key = "system",
            Label = "系统推荐（按平台自动选择）",
            Description = "Windows 使用 Segoe UI，macOS 使用系统字体，Linux 使用 Noto Sans / DejaVu Sans 回退链。"
        });
        FontOptions.Add(new DesignerOptionItem
        {
            Key = "inter",
            Label = "Inter",
            Description = "跨平台一致，但在当前 Avalonia 版本下可能出现输入光标与末尾字符重合问题。"
        });

        if (OperatingSystem.IsWindows())
        {
            FontOptions.Add(new DesignerOptionItem { Key = "segoe-ui", Label = "Segoe UI", Description = "Windows 默认界面字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "microsoft-yahei-ui", Label = "Microsoft YaHei UI", Description = "适合中文界面的 Windows 字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "arial", Label = "Arial", Description = "经典西文字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "bahnschrift", Label = "Bahnschrift", Description = "Windows 自带现代无衬线字体。" });
        }
    }
}

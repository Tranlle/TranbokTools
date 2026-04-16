using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using TOrbit.Core.Models;
using TOrbit.Core.Services;
using TOrbit.Designer.Models;
using TOrbit.Designer.Services;

namespace TOrbit.Plugin.Settings.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IThemeService _themeService;
    private readonly IAppPreferencesService _preferencesService;
    private readonly IPluginCatalogService _pluginCatalog;
    private readonly IPluginVariableService _variableService;

    // ── 主题设置 ──────────────────────────────────────────────────────────────

    [ObservableProperty]
    private string appName = "T-Orbit";

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
    private bool minimizeToTrayOnClose = true;

    [ObservableProperty]
    private string statusMessage = "设置已就绪";

    public ObservableCollection<DesignerOptionItem> FontOptions    { get; } = [];
    public ObservableCollection<DesignerOptionItem> PaletteOptions { get; } = [];
    public ObservableCollection<DesignerOptionItem> AdvancedPaletteOptions { get; } = [];

    public bool   IsInterFontWarningVisible => SelectedFontOption?.Key == "inter";
    public string FontWarningMessage => "Inter 在当前 Avalonia 版本下存在已知 TextBox 光标重合风险；若你看到输入框末尾光标压到最后一个字符，建议切回\u201c系统推荐\u201d。";

    // ── 插件变量管理 ──────────────────────────────────────────────────────────

    // 内部扁平列表（保存/加载的基础数据源）
    private readonly ObservableCollection<PluginVariableItemViewModel> _pluginVariableItems = [];

    // 对外暴露的分组视图（每次列表变动后重建）
    public IReadOnlyList<PluginVariableGroupViewModel> PluginVariableGroups { get; private set; } = [];

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

    public bool   HasPluginVariables => _pluginVariableItems.Count > 0;
    public bool   HasAddFormKeyHints => AddFormKeyHints.Count > 0;

    public string PluginVariableSummary => _pluginVariableItems.Count == 0
        ? "尚未配置任何插件变量"
        : $"共 {_pluginVariableItems.Count} 条变量，跨 {_pluginVariableItems.Select(x => x.PluginId).Distinct().Count()} 个插件";

    // ── 命令 ──────────────────────────────────────────────────────────────────

    public IRelayCommand SaveCommand              { get; }
    public IRelayCommand ResetCommand             { get; }
    public IRelayCommand ShowAddVariableFormCommand { get; }
    public IRelayCommand AddVariableCommand       { get; }
    public IRelayCommand CancelAddVariableCommand { get; }
    public IRelayCommand<DesignerOptionItem> FillKeyFromHintCommand { get; }

    public SettingsViewModel(
        IAppShellService shellService,
        IThemeService themeService,
        IAppPreferencesService preferencesService,
        IPluginCatalogService pluginCatalog,
        IPluginVariableService variableService)
    {
        _themeService       = themeService;
        _preferencesService = preferencesService;
        _pluginCatalog      = pluginCatalog;
        _variableService    = variableService;

        var preferences = _preferencesService.Load();

        InitializeFontOptions();
        InitializePluginOptions();
        LoadPluginVariables();

        appName       = shellService.AppName;
        workspaceRoot = shellService.WorkspaceRoot;
        SelectedFontOption  = FontOptions.FirstOrDefault(o => o.Key == preferences.FontOptionKey)
            ?? FontOptions.FirstOrDefault(o => o.Key == themeService.CurrentFontOptionKey)
            ?? FontOptions.FirstOrDefault();
        MinimizeToTrayOnClose = preferences.CloseButtonBehavior == CloseButtonBehavior.MinimizeToTray;

        foreach (var palette in themeService.GetAvailablePalettes())
        {
            var option = new DesignerOptionItem
            {
                Key   = palette.Key,
                Label = palette.Label,
                Description = palette.Description,
                Value = palette
            };
            if (palette.IsBuiltIn) PaletteOptions.Add(option);
            else                   AdvancedPaletteOptions.Add(option);
        }

        SelectedPaletteOption = PaletteOptions.FirstOrDefault(o => o.Key == themeService.CurrentPaletteKey)
            ?? PaletteOptions.FirstOrDefault();
        SelectedAdvancedPaletteOption = AdvancedPaletteOptions.FirstOrDefault(o => o.Key == themeService.CurrentPaletteKey);

        // ── 命令实现 ─────────────────────────────────────────────────────────

        SaveCommand = new RelayCommand(() =>
        {
            var paletteKey = ShowAdvancedThemeSettings
                ? SelectedAdvancedPaletteOption?.Key ?? SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey
                : SelectedPaletteOption?.Key ?? _themeService.CurrentPaletteKey;
            var fontOptionKey = SelectedFontOption?.Key ?? "system";

            _themeService.SetPalette(paletteKey);
            _themeService.SetFontOption(fontOptionKey);

            var appliedPaletteKey = _themeService.CurrentPaletteKey;
            SelectedPaletteOption = PaletteOptions.FirstOrDefault(o => o.Key == appliedPaletteKey) ?? SelectedPaletteOption;
            SelectedAdvancedPaletteOption = AdvancedPaletteOptions.FirstOrDefault(o => o.Key == appliedPaletteKey);

            _preferencesService.Save(new AppPreferences
            {
                FontOptionKey = fontOptionKey,
                PaletteKey = appliedPaletteKey,
                CloseButtonBehavior = MinimizeToTrayOnClose ? CloseButtonBehavior.MinimizeToTray : CloseButtonBehavior.Exit
            });
            SavePluginVariables();

            var fontLabel = SelectedFontOption?.Label ?? "系统推荐";
            StatusMessage = $"设置已保存，当前方案：{(ShowAdvancedThemeSettings ? SelectedAdvancedPaletteOption?.Label : SelectedPaletteOption?.Label) ?? "默认"} · 字体：{fontLabel}";
        });

        ResetCommand = new RelayCommand(() =>
        {
            AppName = shellService.AppName;
            SelectedPaletteOption = PaletteOptions.FirstOrDefault(o => o.Key == "tranbok-dark") ?? PaletteOptions.FirstOrDefault();
            SelectedAdvancedPaletteOption = null;
            SelectedFontOption = FontOptions.FirstOrDefault(o => o.Key == "system") ?? FontOptions.FirstOrDefault();
            MinimizeToTrayOnClose = true;
            ShowAdvancedThemeSettings = false;
            WorkspaceRoot = shellService.WorkspaceRoot;
            UseWorkspaceForMigrations = true;
            _themeService.SetPalette(SelectedPaletteOption?.Key ?? "tranbok-dark");
            _themeService.SetFontOption("system");
            _preferencesService.Save(new AppPreferences
            {
                FontOptionKey = "system",
                PaletteKey = SelectedPaletteOption?.Key ?? "tranbok-dark",
                CloseButtonBehavior = CloseButtonBehavior.MinimizeToTray
            });
            StatusMessage = "设置已重置";
        });

        ShowAddVariableFormCommand = new RelayCommand(() =>
        {
            InitializePluginOptions();
            AddFormSelectedPlugin = PluginOptions.FirstOrDefault();
            AddFormKey   = string.Empty;
            AddFormValue = string.Empty;
            ShowAddVariableForm = true;
        });

        AddVariableCommand = new RelayCommand(() =>
        {
            var pluginId = AddFormSelectedPlugin?.Key ?? string.Empty;
            var key      = AddFormKey.Trim();
            var value    = AddFormValue;

            if (string.IsNullOrWhiteSpace(pluginId) || string.IsNullOrWhiteSpace(key))
                return;

            var existing = _pluginVariableItems.FirstOrDefault(x =>
                x.PluginId == pluginId && x.Key == key);

            if (existing is not null)
            {
                existing.Value = value;
            }
            else
            {
                var pluginName = AddFormSelectedPlugin?.Label ?? pluginId;
                var hint = AddFormKeyHints.FirstOrDefault(h => h.Key == key);
                var hintData    = hint?.Value as KeyHintData;
                var description = hintData?.Description ?? string.Empty;
                var defaultValue = hintData?.DefaultValue ?? string.Empty;
                var isEncrypted  = hintData?.IsEncrypted ?? false;

                _pluginVariableItems.Add(new PluginVariableItemViewModel(
                    pluginId:      pluginId,
                    pluginName:    pluginName,
                    key:           key,
                    value:         value,
                    defaultValue:  defaultValue,
                    description:   description,
                    isFromMetadata: hint is not null,
                    isEncrypted:   isEncrypted,
                    onDelete:      RemovePluginVariable));
            }

            RebuildGroups();
            ShowAddVariableForm = false;
            AddFormKey   = string.Empty;
            AddFormValue = string.Empty;
        });

        CancelAddVariableCommand = new RelayCommand(() =>
        {
            ShowAddVariableForm = false;
            AddFormKey   = string.Empty;
            AddFormValue = string.Empty;
        });

        FillKeyFromHintCommand = new RelayCommand<DesignerOptionItem>(hint =>
        {
            if (hint is null) return;
            AddFormKey   = hint.Key;
            AddFormValue = hint.Value is KeyHintData d ? d.DefaultValue : hint.Value as string ?? string.Empty;
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
        if (value is null) return;

        var entry = _pluginCatalog.Plugins.FirstOrDefault(p => p.Id == value.Key);
        var definitions = entry?.Plugin.Descriptor.VariableDefinitions;
        if (definitions is null) return;

        foreach (var def in definitions)
        {
            AddFormKeyHints.Add(new DesignerOptionItem
            {
                Key   = def.Key,
                Label = string.IsNullOrWhiteSpace(def.DisplayName) ? def.Key : def.DisplayName,
                Description = def.Description,
                Value = new KeyHintData(def.DefaultValue, def.Description, def.IsEncrypted)
            });
        }

        OnPropertyChanged(nameof(HasAddFormKeyHints));
    }

    private void RemovePluginVariable(PluginVariableItemViewModel item)
    {
        _pluginVariableItems.Remove(item);
        RebuildGroups();
    }

    private void RebuildGroups()
    {
        PluginVariableGroups = _pluginVariableItems
            .GroupBy(x => x.PluginId)
            .OrderBy(g => g.First().PluginName)
            .Select(g => new PluginVariableGroupViewModel
            {
                PluginId   = g.Key,
                PluginName = g.First().PluginName,
                Variables  = g.ToList()
            })
            .ToList();

        OnPropertyChanged(nameof(PluginVariableGroups));
        OnPropertyChanged(nameof(HasPluginVariables));
        OnPropertyChanged(nameof(PluginVariableSummary));
    }

    private void LoadPluginVariables()
    {
        _pluginVariableItems.Clear();
        var store = _variableService.Load();

        // 先将已保存的条目加载进来
        foreach (var entry in store.Entries)
        {
            var plugin     = _pluginCatalog.Plugins.FirstOrDefault(p => p.Id == entry.PluginId);
            var pluginName = plugin?.Name ?? entry.PluginId;
            var def = plugin?.Plugin.Descriptor.VariableDefinitions?
                .FirstOrDefault(d => d.Key == entry.Key);

            // 只有加密字段才需要调 GetValue（走解密路径）；明文直接用已加载的值
            var displayValue = entry.IsEncrypted
                ? _variableService.GetValue(entry.PluginId, entry.Key) ?? entry.Value
                : entry.Value;

            _pluginVariableItems.Add(new PluginVariableItemViewModel(
                pluginId:      entry.PluginId,
                pluginName:    pluginName,
                key:           entry.Key,
                value:         displayValue,
                defaultValue:  def?.DefaultValue ?? string.Empty,
                description:   def?.Description  ?? string.Empty,
                isFromMetadata: def is not null,
                isEncrypted:   def?.IsEncrypted ?? entry.IsEncrypted,
                onDelete:      RemovePluginVariable));
        }

        // 用 HashSet 做 O(1) 去重检查，避免对每个 def 都线性扫描 _pluginVariableItems
        var loadedKeys = _pluginVariableItems
            .Select(x => (x.PluginId, x.Key.ToLowerInvariant()))
            .ToHashSet();

        // 再将元数据声明的但尚未存储的变量补充进来（以默认值填充）
        foreach (var pluginEntry in _pluginCatalog.Plugins)
        {
            var definitions = pluginEntry.Plugin.Descriptor.VariableDefinitions;
            if (definitions is null or { Count: 0 }) continue;

            foreach (var def in definitions)
            {
                if (!loadedKeys.Add((pluginEntry.Id, def.Key.ToLowerInvariant())))
                    continue; // Add 返回 false 说明已存在，跳过

                _pluginVariableItems.Add(new PluginVariableItemViewModel(
                    pluginId:      pluginEntry.Id,
                    pluginName:    pluginEntry.Name,
                    key:           def.Key,
                    value:         def.DefaultValue,
                    defaultValue:  def.DefaultValue,
                    description:   def.Description,
                    isFromMetadata: true,
                    isEncrypted:   def.IsEncrypted,
                    onDelete:      RemovePluginVariable));
            }
        }

        RebuildGroups();
    }

    private void SavePluginVariables()
    {
        var store = new PluginVariableStore
        {
            Entries = _pluginVariableItems
                .Select(item => new PluginVariableEntry
                {
                    PluginId    = item.PluginId,
                    Key         = item.Key,
                    Value       = item.Value,       // 明文；加密在 Service.Save 内完成
                    IsEncrypted = item.IsEncrypted
                })
                .ToList()
        };
        _variableService.Save(store);       // Save 内部按 IsEncrypted 加密后写盘
        _variableService.InjectAll();       // 保存后重新注入所有插件
    }

    private void InitializePluginOptions()
    {
        PluginOptions.Clear();
        foreach (var plugin in _pluginCatalog.Plugins.OrderBy(p => p.Name))
        {
            PluginOptions.Add(new DesignerOptionItem
            {
                Key   = plugin.Id,
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
            Key   = "system",
            Label = "系统推荐（按平台自动选择）",
            Description = "Windows 使用 Segoe UI，macOS 使用系统字体，Linux 使用 Noto Sans / DejaVu Sans 回退链。"
        });
        FontOptions.Add(new DesignerOptionItem
        {
            Key   = "inter",
            Label = "Inter",
            Description = "跨平台一致，但在当前 Avalonia 版本下可能出现输入光标与末尾字符重合问题。"
        });

        if (OperatingSystem.IsWindows())
        {
            FontOptions.Add(new DesignerOptionItem { Key = "segoe-ui",          Label = "Segoe UI",          Description = "Windows 默认界面字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "microsoft-yahei-ui", Label = "Microsoft YaHei UI", Description = "适合中文界面的 Windows 字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "arial",              Label = "Arial",             Description = "经典西文字体。" });
            FontOptions.Add(new DesignerOptionItem { Key = "bahnschrift",        Label = "Bahnschrift",       Description = "Windows 自带现代无衬线字体。" });
        }
    }

    // ── 嵌套辅助类型 ──────────────────────────────────────────────────────────

    /// <summary>存储在 AddFormKeyHints 的 Value 中，携带默认值、说明和加密标志。</summary>
    private sealed record KeyHintData(string DefaultValue, string Description, bool IsEncrypted);
}

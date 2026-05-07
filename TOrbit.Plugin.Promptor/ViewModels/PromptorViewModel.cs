using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TOrbit.Designer.Models;
using TOrbit.Designer.Services;
using TOrbit.Designer.ViewModels;
using TOrbit.Designer.ViewModels.Dialogs;
using TOrbit.Plugin.Promptor.Models;
using TOrbit.Plugin.Promptor.Services;
using TOrbit.Plugin.Promptor.Views;

namespace TOrbit.Plugin.Promptor.ViewModels;

public sealed partial class PromptorViewModel : PluginBaseViewModel, IDisposable
{
    private readonly IDesignerDialogService? _dialogService;
    private readonly Action<PromptorVariables> _saveVariables;
    private readonly ILocalizationService _localizationService;
    private readonly PromptOptimizationService _service = new();
    private PromptorVariables _variables;
    private CancellationTokenSource? _cts;
    private CancellationTokenSource? _copyCts;

    public event EventHandler? HeaderSummaryChanged;

    [ObservableProperty]
    private string rawInput = string.Empty;

    [ObservableProperty]
    private string optimizedOutput = string.Empty;

    [ObservableProperty]
    private DesignerOptionItem? selectedStrategyOption;

    [ObservableProperty]
    private bool isBusy;

    [ObservableProperty]
    private bool isCopied;

    [ObservableProperty]
    private string statusMessage;

    public ObservableCollection<PromptorLogEntry> LogEntries { get; } = [];
    public ObservableCollection<PromptorLogEntry> PagedLogEntries { get; } = [];

    public string FormattedLogText => LogEntries.Count == 0
        ? $"{L("promptor.logEmpty")}{Environment.NewLine}{Environment.NewLine}{L("promptor.logEmptyDescription")}"
        : string.Join(Environment.NewLine + Environment.NewLine, LogEntries.Select(entry => entry.FormatAsText()));

    public ObservableCollection<DesignerOptionItem> StrategyOptions { get; } = [];

    public bool HasRawInput => !string.IsNullOrWhiteSpace(RawInput);
    public bool HasOptimizedOutput => !string.IsNullOrWhiteSpace(OptimizedOutput);
    public bool IsIdle => !IsBusy;
    public bool CanOptimize => HasRawInput && !IsBusy;
    public string StrategyDescription => SelectedStrategyOption?.Description ?? string.Empty;
    public string CopyButtonText => IsCopied ? L("promptor.copied") : L("promptor.copy");
    public int LogCount => LogEntries.Count;
    public string LogEntriesSummary => string.Format(L("promptor.logEntriesFormat"), LogEntries.Count);
    public int LogPageSize => 5;
    public int TotalLogPages => Math.Max(1, (int)Math.Ceiling(LogEntries.Count / (double)LogPageSize));
    public string LogPageSummary => string.Format(L("promptor.logPageFormat"), CurrentLogPage, TotalLogPages);
    public bool HasLogEntries => LogEntries.Count > 0;
    public bool HasPreviousLogPage => CurrentLogPage > 1;
    public bool HasNextLogPage => CurrentLogPage < TotalLogPages;

    [ObservableProperty]
    private int currentLogPage = 1;

    public IRelayCommand OptimizeCommand { get; }
    public IRelayCommand CopyCommand { get; }
    public IRelayCommand ClearAllCommand { get; }
    public IRelayCommand ClearOutputCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand ShowLogCommand { get; }
    public IRelayCommand ClearLogCommand { get; }
    public IRelayCommand PreviousLogPageCommand { get; }
    public IRelayCommand NextLogPageCommand { get; }
    public IRelayCommand<PromptorLogEntry> ShowLogDetailCommand { get; }
    public IRelayCommand OpenApiSettingsCommand { get; }

    public PromptorViewModel(
        IDesignerDialogService? dialogService,
        Action<PromptorVariables> saveVariables,
        PromptorVariables variables,
        ILocalizationService localizationService)
    {
        _dialogService = dialogService;
        _saveVariables = saveVariables;
        _variables = variables;
        _localizationService = localizationService;
        statusMessage = L("runtime.ready");

        InitializeStrategyOptions();
        SelectDefaultStrategy();

        OptimizeCommand = new AsyncRelayCommand(OptimizeAsync);
        CopyCommand = new AsyncRelayCommand(CopyToClipboardAsync);
        ClearAllCommand = new RelayCommand(ClearAll);
        ClearOutputCommand = new RelayCommand(() => OptimizedOutput = string.Empty);
        CancelCommand = new RelayCommand(() => _cts?.Cancel());
        ShowLogCommand = new AsyncRelayCommand(ShowLogDialogAsync);
        ClearLogCommand = new RelayCommand(ClearLog);
        PreviousLogPageCommand = new RelayCommand(PreviousLogPage);
        NextLogPageCommand = new RelayCommand(NextLogPage);
        ShowLogDetailCommand = new AsyncRelayCommand<PromptorLogEntry>(ShowLogDetailAsync);
        OpenApiSettingsCommand = new AsyncRelayCommand(OpenApiSettingsAsync);

        LogEntries.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(FormattedLogText));
            OnPropertyChanged(nameof(LogCount));
            OnPropertyChanged(nameof(LogEntriesSummary));
            OnPropertyChanged(nameof(TotalLogPages));
            OnPropertyChanged(nameof(LogPageSummary));
            OnPropertyChanged(nameof(HasLogEntries));
            if (CurrentLogPage > TotalLogPages)
                CurrentLogPage = TotalLogPages;
            RefreshPagedLogEntries();
            HeaderSummaryChanged?.Invoke(this, EventArgs.Empty);
        };
        RefreshPagedLogEntries();
    }

    partial void OnRawInputChanged(string value) => RaiseDerivedProperties();
    partial void OnOptimizedOutputChanged(string value) => RaiseDerivedProperties();
    partial void OnIsBusyChanged(bool value) => RaiseDerivedProperties();

    partial void OnIsCopiedChanged(bool value)
    {
        OnPropertyChanged(nameof(CopyButtonText));
        HeaderSummaryChanged?.Invoke(this, EventArgs.Empty);
    }

    partial void OnSelectedStrategyOptionChanged(DesignerOptionItem? value)
    {
        if (value is null && StrategyOptions.Count > 0)
        {
            SelectDefaultStrategy();
            return;
        }

        RaiseDerivedProperties();
    }
    partial void OnCurrentLogPageChanged(int value)
    {
        OnPropertyChanged(nameof(LogPageSummary));
        OnPropertyChanged(nameof(HasPreviousLogPage));
        OnPropertyChanged(nameof(HasNextLogPage));
        RefreshPagedLogEntries();
    }

    public void UpdateVariables(PromptorVariables variables)
    {
        _variables = variables;
        HeaderSummaryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void RaiseDerivedProperties()
    {
        OnPropertyChanged(nameof(HasRawInput));
        OnPropertyChanged(nameof(HasOptimizedOutput));
        OnPropertyChanged(nameof(IsIdle));
        OnPropertyChanged(nameof(CanOptimize));
        OnPropertyChanged(nameof(StrategyDescription));
        HeaderSummaryChanged?.Invoke(this, EventArgs.Empty);
    }

    private async Task OptimizeAsync()
    {
        if (string.IsNullOrWhiteSpace(RawInput))
        {
            await ShowAlertAsync(L("promptor.messages.inputRequiredTitle"), L("promptor.messages.inputRequired"));
            return;
        }

        PromptorConfig config;
        try
        {
            config = ReadConfig();
        }
        catch (Exception ex)
        {
            await ShowAlertAsync(L("promptor.messages.configError"), ex.Message);
            return;
        }

        var strategy = SelectedStrategyOption?.Value is OptimizationStrategy selected
            ? selected
            : OptimizationStrategy.TaskExecution;
        var strategyLabel = SelectedStrategyOption?.Label ?? L("promptor.strategy.taskExecution.label");
        var inputPreview = RawInput.Length > 80
            ? RawInput[..80].Replace('\n', ' ').Replace('\r', ' ') + "..."
            : RawInput.Replace('\n', ' ').Replace('\r', ' ');

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        IsBusy = true;
        OptimizedOutput = string.Empty;
        StatusMessage = L("promptor.messages.optimizing");

        var sw = Stopwatch.StartNew();
        var success = false;
        string? errorMessage = null;

        try
        {
            await foreach (var chunk in _service.OptimizeStreamAsync(RawInput, strategy, config, _cts.Token))
            {
                var captured = chunk;
                await Dispatcher.UIThread.InvokeAsync(() => OptimizedOutput += captured);
            }

            sw.Stop();
            success = true;
            StatusMessage = L("promptor.messages.completed");
        }
        catch (OperationCanceledException)
        {
            sw.Stop();
            StatusMessage = L("promptor.messages.cancelled");
            errorMessage = L("promptor.messages.cancelledByUser");
        }
        catch (Exception ex)
        {
            sw.Stop();
            StatusMessage = L("promptor.messages.failed");
            errorMessage = ex.Message;
            await ShowAlertAsync(L("promptor.messages.optimizeFailed"), ex.Message);
        }
        finally
        {
            IsBusy = false;
            RaiseDerivedProperties();

            LogEntries.Insert(0, new PromptorLogEntry
            {
                Time = DateTime.Now,
                StrategyLabel = strategyLabel,
                Model = config.ModelName,
                Provider = config.Provider,
                Endpoint = ResolveEndpoint(config),
                IsSuccess = success,
                Duration = sw.Elapsed,
                MaxTokens = config.MaxTokens,
                Temperature = config.Temperature,
                InputPreview = inputPreview,
                Input = RawInput,
                Output = OptimizedOutput,
                ErrorMessage = errorMessage
            });
        }
    }

    private PromptorConfig ReadConfig()
    {
        var provider = string.IsNullOrWhiteSpace(_variables.Provider) ? "openai" : _variables.Provider.Trim();
        var endpoint = _variables.ApiEndpoint?.Trim() ?? string.Empty;
        var apiKey = _variables.ApiKey?.Trim() ?? string.Empty;
        var model = string.IsNullOrWhiteSpace(_variables.ModelName) ? "gpt-4o" : _variables.ModelName.Trim();
        var maxTokens = _variables.MaxTokens > 0 ? _variables.MaxTokens : 2048;
        var temperature = double.IsFinite(_variables.Temperature) ? _variables.Temperature : 1.0;

        var isOllama = string.Equals(provider, "ollama", StringComparison.OrdinalIgnoreCase);
        var hasEndpoint = !string.IsNullOrWhiteSpace(endpoint);

        if (!isOllama && !hasEndpoint && string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException(L("promptor.messages.apiKeyRequired"));

        return new PromptorConfig(provider, endpoint, apiKey, model, maxTokens, temperature);
    }

    private async Task ShowLogDialogAsync()
    {
        if (_dialogService is null || TryGetOwnerWindow() is not { } owner)
            return;

        var content = new LogDialogView { DataContext = this };

        await _dialogService.ShowSheetAsync(owner, new DesignerSheetViewModel
        {
            Title = L("promptor.log"),
            Description = LogEntries.Count > 0
                ? string.Format(L("promptor.logEntriesFormat"), LogEntries.Count)
                : L("promptor.logEmpty"),
            Content = content,
            ConfirmText = L("dialog.close"),
            CancelText = string.Empty,
            Icon = DesignerDialogIcon.Info,
            BaseFontSize = 13,
            DialogWidth = 860,
            DialogHeight = 560,
            LockSize = true,
            HideSystemDecorations = true
        });
    }

    private async Task CopyToClipboardAsync()
    {
        if (string.IsNullOrEmpty(OptimizedOutput))
            return;

        try
        {
            if (TryGetOwnerWindow()?.Clipboard is { } clipboard)
            {
                await clipboard.SetTextAsync(OptimizedOutput);
                StatusMessage = L("promptor.messages.copied");

                _copyCts?.Cancel();
                _copyCts = new CancellationTokenSource();
                var token = _copyCts.Token;
                IsCopied = true;

                try
                {
                    await Task.Delay(1500, token);
                    IsCopied = false;
                }
                catch (OperationCanceledException)
                {
                }
            }
        }
        catch
        {
            StatusMessage = L("promptor.messages.copyFailed");
        }
    }

    private void ClearAll()
    {
        RawInput = string.Empty;
        OptimizedOutput = string.Empty;
        StatusMessage = L("runtime.ready");
        RaiseDerivedProperties();
    }

    private void ClearLog()
    {
        LogEntries.Clear();
        CurrentLogPage = 1;
    }

    private void PreviousLogPage()
    {
        if (CurrentLogPage > 1)
            CurrentLogPage--;
    }

    private void NextLogPage()
    {
        if (CurrentLogPage < TotalLogPages)
            CurrentLogPage++;
    }

    private void RefreshPagedLogEntries()
    {
        PagedLogEntries.Clear();
        foreach (var entry in LogEntries.Skip((CurrentLogPage - 1) * LogPageSize).Take(LogPageSize))
            PagedLogEntries.Add(entry);

        OnPropertyChanged(nameof(HasPreviousLogPage));
        OnPropertyChanged(nameof(HasNextLogPage));
    }

    private async Task ShowLogDetailAsync(PromptorLogEntry? entry)
    {
        if (entry is null || _dialogService is null || TryGetOwnerWindow() is not { } owner)
            return;

        var content = new LogDetailView { DataContext = entry };
        await _dialogService.ShowSheetAsync(owner, new DesignerSheetViewModel
        {
            Title = L("promptor.logDetail"),
            Description = $"{entry.TimeText} · {entry.ModelText}",
            Content = content,
            ConfirmText = L("dialog.close"),
            CancelText = string.Empty,
            Icon = DesignerDialogIcon.Info,
            BaseFontSize = 13,
            DialogWidth = 900,
            DialogHeight = 720,
            LockSize = true,
            HideSystemDecorations = true
        });
    }

    private async Task OpenApiSettingsAsync()
    {
        if (_dialogService is null || TryGetOwnerWindow() is not { } owner)
            return;

        var viewModel = new PromptorApiSettingsViewModel(_variables, _localizationService);
        var content = new ApiSettingsSheetView { DataContext = viewModel };

        var result = await _dialogService.ShowSheetAsync(owner, new DesignerSheetViewModel
        {
            Title = L("promptor.apiSettings"),
            Description = L("promptor.apiSettingsDescription"),
            Content = content,
            ConfirmText = L("dialog.confirm"),
            CancelText = L("dialog.cancel"),
            Icon = DesignerDialogIcon.Info,
            BaseFontSize = 13,
            DialogWidth = 680,
            DialogHeight = 620,
            LockSize = true,
            HideSystemDecorations = true
        });

        if (!result.IsConfirmed)
            return;

        if (!viewModel.TryBuildVariables(out var variables, out var errorMessage))
        {
            await ShowAlertAsync(L("promptor.messages.configError"), errorMessage);
            return;
        }

        _saveVariables(variables);
        StatusMessage = L("promptor.messages.apiSettingsSaved");
    }

    private async Task ShowAlertAsync(string title, string message)
    {
        if (_dialogService is null || TryGetOwnerWindow() is not { } owner)
            return;

        await _dialogService.ShowConfirmAsync(owner, new DesignerConfirmDialogViewModel
        {
            Title = title,
            Message = message,
            ConfirmText = "OK",
            CancelText = string.Empty,
            Icon = DesignerDialogIcon.Info
        });
    }

    private void InitializeStrategyOptions()
    {
        StrategyOptions.Clear();

        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "TaskExecution",
            Label = L("promptor.strategy.taskExecution.label"),
            Value = OptimizationStrategy.TaskExecution,
            Description = L("promptor.strategy.taskExecution.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "Coding",
            Label = L("promptor.strategy.coding.label"),
            Value = OptimizationStrategy.Coding,
            Description = L("promptor.strategy.coding.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "Writing",
            Label = L("promptor.strategy.writing.label"),
            Value = OptimizationStrategy.Writing,
            Description = L("promptor.strategy.writing.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "ResearchAnalysis",
            Label = L("promptor.strategy.researchAnalysis.label"),
            Value = OptimizationStrategy.ResearchAnalysis,
            Description = L("promptor.strategy.researchAnalysis.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "Extraction",
            Label = L("promptor.strategy.extraction.label"),
            Value = OptimizationStrategy.Extraction,
            Description = L("promptor.strategy.extraction.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "ReviewEvaluation",
            Label = L("promptor.strategy.reviewEvaluation.label"),
            Value = OptimizationStrategy.ReviewEvaluation,
            Description = L("promptor.strategy.reviewEvaluation.description")
        });
        StrategyOptions.Add(new DesignerOptionItem
        {
            Key = "AgentWorkflow",
            Label = L("promptor.strategy.agentWorkflow.label"),
            Value = OptimizationStrategy.AgentWorkflow,
            Description = L("promptor.strategy.agentWorkflow.description")
        });
    }

    private void SelectDefaultStrategy()
    {
        SelectedStrategyOption = StrategyOptions.FirstOrDefault(option =>
            option.Value is OptimizationStrategy.TaskExecution) ?? StrategyOptions.FirstOrDefault();
    }

    private string L(string key) => _localizationService.GetString(key);

    private static string ResolveEndpoint(PromptorConfig config)
    {
        if (!string.IsNullOrWhiteSpace(config.ApiEndpoint))
        {
            var ep = config.ApiEndpoint.TrimEnd('/');
            if (!ep.EndsWith("/chat/completions", StringComparison.OrdinalIgnoreCase))
                ep += "/chat/completions";
            return ep;
        }

        return config.Provider.ToLowerInvariant() switch
        {
            "openai" => "https://api.openai.com/v1/chat/completions",
            "qwen" => "https://dashscope.aliyuncs.com/compatible-mode/v1/chat/completions",
            "kimi" => "https://api.moonshot.cn/v1/chat/completions",
            "ollama" => "http://localhost:11434/v1/chat/completions",
            _ => string.Empty
        };
    }

    private static Window? TryGetOwnerWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            return desktop.MainWindow;

        return null;
    }

    public void Dispose()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _copyCts?.Cancel();
        _copyCts?.Dispose();
        _service.Dispose();
    }
}

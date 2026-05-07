using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using TOrbit.Designer.Models;
using TOrbit.Designer.Services;
using TOrbit.Plugin.Promptor.Models;

namespace TOrbit.Plugin.Promptor.ViewModels;

public sealed partial class PromptorApiSettingsViewModel : ObservableObject
{
    private readonly ILocalizationService _localizationService;
    private bool _isInitializing;

    [ObservableProperty]
    private DesignerOptionItem? selectedProvider;

    [ObservableProperty]
    private string apiEndpoint = string.Empty;

    [ObservableProperty]
    private string apiKey = string.Empty;

    [ObservableProperty]
    private string modelName = "gpt-4o";

    [ObservableProperty]
    private string maxTokens = "2048";

    [ObservableProperty]
    private string temperature = "1.0";

    [ObservableProperty]
    private string presetEndpoint = string.Empty;

    [ObservableProperty]
    private string presetModelName = string.Empty;

    [ObservableProperty]
    private string presetSummary = string.Empty;

    public ObservableCollection<DesignerOptionItem> ProviderOptions { get; } =
    [
        new() { Key = "openai", Label = "OpenAI" },
        new() { Key = "qwen", Label = "Qwen" },
        new() { Key = "kimi", Label = "Kimi" },
        new() { Key = "ollama", Label = "Ollama" }
    ];

    public PromptorApiSettingsViewModel(PromptorVariables variables, ILocalizationService localizationService)
    {
        _localizationService = localizationService;
        _isInitializing = true;
        SelectedProvider = ProviderOptions.FirstOrDefault(option =>
            string.Equals(option.Key, variables.Provider, StringComparison.OrdinalIgnoreCase))
            ?? ProviderOptions.FirstOrDefault();
        var preset = GetPreset(SelectedProvider?.Key);
        ApiEndpoint = IsPresetEndpoint(variables.ApiEndpoint) ? string.Empty : variables.ApiEndpoint;
        ApiKey = variables.ApiKey;
        ModelName = ShouldUsePresetModel(variables.ModelName, preset)
            ? preset.ModelName
            : variables.ModelName;
        MaxTokens = (variables.MaxTokens > 0 ? variables.MaxTokens : preset.MaxTokens).ToString(CultureInfo.InvariantCulture);
        Temperature = (double.IsFinite(variables.Temperature) ? variables.Temperature : preset.Temperature)
            .ToString(CultureInfo.InvariantCulture);
        ApplyPresetText(preset);
        _isInitializing = false;
    }

    partial void OnSelectedProviderChanged(DesignerOptionItem? value)
    {
        var preset = GetPreset(value?.Key);
        ApplyPresetText(preset);

        if (_isInitializing)
            return;

        ApiEndpoint = string.Empty;
        ApiKey = string.Empty;
        ModelName = preset.ModelName;
        MaxTokens = preset.MaxTokens.ToString(CultureInfo.InvariantCulture);
        Temperature = preset.Temperature.ToString(CultureInfo.InvariantCulture);
    }

    public bool TryBuildVariables(out PromptorVariables variables, out string errorMessage)
    {
        variables = new PromptorVariables();
        errorMessage = string.Empty;

        var provider = SelectedProvider?.Key ?? string.Empty;
        if (string.IsNullOrWhiteSpace(provider))
        {
            errorMessage = L("promptor.apiSettingsValidation.provider");
            return false;
        }

        if (string.IsNullOrWhiteSpace(ModelName))
        {
            errorMessage = L("promptor.apiSettingsValidation.model");
            return false;
        }

        if (!int.TryParse(MaxTokens, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedMaxTokens)
            || parsedMaxTokens <= 0)
        {
            errorMessage = L("promptor.apiSettingsValidation.maxTokens");
            return false;
        }

        if (!double.TryParse(Temperature, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedTemperature)
            || !double.IsFinite(parsedTemperature)
            || parsedTemperature < 0
            || parsedTemperature > 2)
        {
            errorMessage = L("promptor.apiSettingsValidation.temperature");
            return false;
        }

        variables = new PromptorVariables
        {
            Provider = provider,
            ApiEndpoint = IsPresetEndpoint(ApiEndpoint) ? string.Empty : ApiEndpoint.Trim(),
            ApiKey = ApiKey.Trim(),
            ModelName = ModelName.Trim(),
            MaxTokens = parsedMaxTokens,
            Temperature = parsedTemperature
        };
        return true;
    }

    private string L(string key) => _localizationService.GetString(key);

    private void ApplyPresetText(ProviderPreset preset)
    {
        PresetEndpoint = preset.Endpoint;
        PresetModelName = preset.ModelName;
        PresetSummary = string.Format(
            CultureInfo.CurrentCulture,
            L("promptor.apiSettingsPresetSummary"),
            preset.Endpoint,
            preset.ModelName,
            preset.MaxTokens,
            preset.Temperature);
    }

    private static bool ShouldUsePresetModel(string modelName, ProviderPreset preset)
    {
        if (string.IsNullOrWhiteSpace(modelName))
            return true;

        return GetPresets().Any(item => string.Equals(item.ModelName, modelName, StringComparison.OrdinalIgnoreCase))
            && !string.Equals(modelName, preset.ModelName, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsPresetEndpoint(string endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint))
            return true;

        var normalized = endpoint.Trim().TrimEnd('/');
        return GetPresets().Any(item =>
            string.Equals(item.Endpoint.TrimEnd('/'), normalized, StringComparison.OrdinalIgnoreCase)
            || string.Equals($"{item.Endpoint.TrimEnd('/')}/chat/completions", normalized, StringComparison.OrdinalIgnoreCase));
    }

    private static ProviderPreset GetPreset(string? provider)
        => GetPresets().FirstOrDefault(item => string.Equals(item.Provider, provider, StringComparison.OrdinalIgnoreCase))
            ?? GetPresets()[0];

    private static IReadOnlyList<ProviderPreset> GetPresets() =>
    [
        new("openai", "https://api.openai.com/v1", "gpt-4o", 2048, 1.0),
        new("qwen", "https://dashscope.aliyuncs.com/compatible-mode/v1", "qwen-plus", 2048, 1.0),
        new("kimi", "https://api.moonshot.cn/v1", "moonshot-v1-8k", 2048, 1.0),
        new("ollama", "http://localhost:11434/v1", "llama3.1", 2048, 0.7)
    ];

    private sealed record ProviderPreset(
        string Provider,
        string Endpoint,
        string ModelName,
        int MaxTokens,
        double Temperature);
}

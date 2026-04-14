using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Tranbok.Tools.Plugin.Settings.ViewModels;

public sealed partial class PluginVariableItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string value;

    public string PluginId { get; }
    public string PluginName { get; }
    public string Key { get; }
    public string DefaultValue { get; }
    public string Description { get; }
    public bool IsFromMetadata { get; }

    public IRelayCommand DeleteCommand { get; }

    public PluginVariableItemViewModel(
        string pluginId,
        string pluginName,
        string key,
        string value,
        string defaultValue = "",
        string description = "",
        bool isFromMetadata = false,
        Action<PluginVariableItemViewModel>? onDelete = null)
    {
        PluginId = pluginId;
        PluginName = pluginName;
        Key = key;
        this.value = value;
        DefaultValue = defaultValue;
        Description = description;
        IsFromMetadata = isFromMetadata;
        DeleteCommand = new RelayCommand(() => onDelete?.Invoke(this));
    }
}

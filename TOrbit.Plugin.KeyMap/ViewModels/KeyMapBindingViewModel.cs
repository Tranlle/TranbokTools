using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TOrbit.Core.Models;
using TOrbit.Core.Services;

namespace TOrbit.Plugin.KeyMap.ViewModels;

public sealed partial class KeyMapBindingViewModel : ObservableObject
{
    private readonly KeyMapEntry _entry;
    private readonly IKeyMapService _keyMapService;

    public string Id => _entry.Id;
    public string PluginId => _entry.PluginId;
    public string PluginName => _entry.PluginName;
    public string Name => _entry.Name;
    public string Description => _entry.Description;
    public string DefaultKey => _entry.DefaultKey;
    public bool IsModified => !string.IsNullOrWhiteSpace(_entry.CustomKey);

    [ObservableProperty]
    private string currentKeyDisplay;

    [ObservableProperty]
    private bool isEnabled;

    [ObservableProperty]
    private bool isSelected;

    public IRelayCommand ResetCommand { get; }

    public KeyMapBindingViewModel(KeyMapEntry entry, IKeyMapService keyMapService)
    {
        _entry = entry;
        _keyMapService = keyMapService;
        currentKeyDisplay = entry.EffectiveKey;
        isEnabled = entry.IsEnabled;

        ResetCommand = new RelayCommand(DoReset);
    }

    partial void OnCurrentKeyDisplayChanged(string value)
    {
        _entry.CustomKey = string.Equals(value, _entry.DefaultKey, StringComparison.OrdinalIgnoreCase)
            ? null
            : value;
        OnPropertyChanged(nameof(IsModified));
    }

    partial void OnIsEnabledChanged(bool value)
    {
        _entry.IsEnabled = value;
    }

    private void DoReset()
    {
        _keyMapService.Reset(_entry.Id);
        CurrentKeyDisplay = _entry.EffectiveKey;
        IsEnabled = _entry.IsEnabled;
        OnPropertyChanged(nameof(IsModified));
    }
}

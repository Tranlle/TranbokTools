using System.Collections.ObjectModel;

namespace Tranbok.Tools.Plugin.KeyMap.ViewModels;

public sealed class KeyMapGroupViewModel
{
    public string PluginName { get; }
    public ObservableCollection<KeyMapBindingViewModel> Bindings { get; }
    public int Count => Bindings.Count;

    public KeyMapGroupViewModel(string pluginName, IEnumerable<KeyMapBindingViewModel> bindings)
    {
        PluginName = pluginName;
        Bindings = new ObservableCollection<KeyMapBindingViewModel>(bindings);
    }
}

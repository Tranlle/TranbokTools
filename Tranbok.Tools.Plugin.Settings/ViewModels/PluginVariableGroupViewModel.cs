using System.Collections.ObjectModel;

namespace Tranbok.Tools.Plugin.Settings.ViewModels;

public sealed class PluginVariableGroupViewModel
{
    public string PluginId { get; init; } = string.Empty;
    public string PluginName { get; init; } = string.Empty;
    public IReadOnlyList<PluginVariableItemViewModel> Variables { get; init; } = [];
    public int Count => Variables.Count;
}

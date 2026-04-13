using System.Windows.Input;

namespace Tranbok.Tools.Plugin.Core.Abstractions;

public interface IPluginViewProvider
{
    object GetMainView();
}

public interface IPluginHeaderActionsProvider
{
    IReadOnlyList<PluginHeaderAction> GetHeaderActions();
}

public sealed record PluginHeaderAction(
    string Label,
    ICommand Command,
    bool IsVisible = true,
    bool IsPrimary = false);

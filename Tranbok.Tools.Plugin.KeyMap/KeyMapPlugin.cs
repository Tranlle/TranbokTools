using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Tranbok.Tools.Core.Services;
using Tranbok.Tools.Designer.Abstractions;
using Tranbok.Tools.Plugin.Core;
using Tranbok.Tools.Plugin.Core.Abstractions;
using Tranbok.Tools.Plugin.Core.Base;
using Tranbok.Tools.Plugin.KeyMap.ViewModels;
using Tranbok.Tools.Plugin.KeyMap.Views;

namespace Tranbok.Tools.Plugin.KeyMap;

public sealed class KeyMapPlugin : BasePlugin, IVisualPlugin, IPluginHeaderActionsProvider
{
    private KeyMapView? _view;
    private KeyMapViewModel? _viewModel;
    private IReadOnlyList<PluginHeaderAction>? _headerActions;

    public override PluginDescriptor Descriptor { get; } =
        CreateDescriptor<KeyMapPlugin>(KeyMapPluginMetadata.Instance);

    public override Control GetMainView()
    {
        EnsureView();
        return _view!;
    }

    public IReadOnlyList<PluginHeaderAction> GetHeaderActions()
    {
        EnsureView();
        return _headerActions ?? [];
    }

    private void EnsureView()
    {
        if (_viewModel is not null)
            return;

        var services = Context.Services ?? throw new InvalidOperationException("Plugin services are unavailable.");
        var keyMapService = services.GetRequiredService<IKeyMapService>();

        _viewModel = new KeyMapViewModel(keyMapService);
        _headerActions =
        [
            new PluginHeaderAction("重置全部", _viewModel.ResetAllCommand),
            new PluginHeaderAction("保存", _viewModel.SaveCommand, IsPrimary: true)
        ];
        _view = new KeyMapView { DataContext = _viewModel };
    }

    protected override ValueTask OnDisposeAsync()
    {
        _view = null;
        _viewModel = null;
        _headerActions = null;
        return ValueTask.CompletedTask;
    }
}

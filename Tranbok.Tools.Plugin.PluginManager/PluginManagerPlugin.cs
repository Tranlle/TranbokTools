using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Tranbok.Tools.Core.Services;
using Tranbok.Tools.Designer.Abstractions;
using Tranbok.Tools.Plugin.Core;
using Tranbok.Tools.Plugin.Core.Base;
using Tranbok.Tools.Plugin.PluginManager.ViewModels;
using Tranbok.Tools.Plugin.PluginManager.Views;

namespace Tranbok.Tools.Plugin.PluginManager;

public sealed class PluginManagerPlugin : BasePlugin, IVisualPlugin
{
    private PluginManagerView? _view;
    private PluginManagerViewModel? _viewModel;

    public override PluginDescriptor Descriptor { get; } = CreateDescriptor<PluginManagerPlugin>(PluginManagerPluginMetadata.Instance);

    protected override ValueTask OnStartAsync(CancellationToken cancellationToken = default)
    {
        EnsureView();
        return ValueTask.CompletedTask;
    }

    public override Control GetMainView()
    {
        EnsureView();
        return _view!;
    }

    private void EnsureView()
    {
        if (_viewModel is null)
        {
            var catalog = Context.Services?.GetRequiredService<IPluginCatalogService>()
                ?? throw new InvalidOperationException("Plugin services are unavailable.");
            _viewModel = new PluginManagerViewModel(catalog);
        }

        _view ??= new PluginManagerView { DataContext = _viewModel };
    }

    protected override ValueTask OnDisposeAsync()
    {
        _view = null;
        _viewModel = null;
        return ValueTask.CompletedTask;
    }
}

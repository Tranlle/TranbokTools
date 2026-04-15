using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using TOrbit.Core.Services;
using TOrbit.Designer.Abstractions;
using TOrbit.Designer.Services;
using TOrbit.Plugin.Core;
using TOrbit.Plugin.Core.Abstractions;
using TOrbit.Plugin.Core.Base;
using TOrbit.Plugin.Settings.ViewModels;
using TOrbit.Plugin.Settings.Views;

namespace TOrbit.Plugin.Settings;

public sealed class SettingsPlugin : BasePlugin, IVisualPlugin, IPluginHeaderActionsProvider
{
    private SettingsView? _view;
    private SettingsViewModel? _viewModel;
    private IReadOnlyList<PluginHeaderAction>? _headerActions;

    public override PluginDescriptor Descriptor { get; } = CreateDescriptor<SettingsPlugin>(SettingsPluginMetadata.Instance);

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
        if (_viewModel is null)
        {
            var services = Context.Services ?? throw new InvalidOperationException("Plugin services are unavailable.");
            var shellService = services.GetRequiredService<IAppShellService>();
            var themeService = services.GetRequiredService<IThemeService>();
            var preferencesService = services.GetRequiredService<IAppPreferencesService>();
            var pluginCatalog = services.GetRequiredService<IPluginCatalogService>();
            var variableService = services.GetRequiredService<IPluginVariableService>();
            _viewModel = new SettingsViewModel(shellService, themeService, preferencesService, pluginCatalog, variableService);
            _headerActions =
            [
                new PluginHeaderAction("重置设置", _viewModel.ResetCommand),
                new PluginHeaderAction("保存设置", _viewModel.SaveCommand, IsPrimary: true)
            ];
        }

        _view ??= new SettingsView { DataContext = _viewModel };
    }

    protected override ValueTask OnDisposeAsync()
    {
        _view = null;
        _viewModel = null;
        _headerActions = null;
        return ValueTask.CompletedTask;
    }
}

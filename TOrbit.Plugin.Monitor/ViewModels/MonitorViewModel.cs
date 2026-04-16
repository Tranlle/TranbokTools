using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using TOrbit.Core.Models;
using TOrbit.Core.Services;
using TOrbit.Plugin.Core.Enums;

namespace TOrbit.Plugin.Monitor.ViewModels;

public sealed partial class MonitorViewModel : ObservableObject
{
    private readonly IPluginCatalogService _pluginCatalog;
    private readonly IPluginLifecycleService _pluginLifecycleService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedMonitorItem))]
    private PluginMonitorItemViewModel? selectedMonitorItem;

    public ObservableCollection<PluginMonitorItemViewModel> MonitorItems { get; } = [];
    public bool HasSelectedMonitorItem => SelectedMonitorItem is not null;
    public int TotalCount => MonitorItems.Count;
    public int RunningCount => MonitorItems.Count(x => x.State == PluginState.Running);
    public int FaultedCount => MonitorItems.Count(x => x.State == PluginState.Faulted);
    public int DisabledCount => MonitorItems.Count(x => !x.IsEnabled);
    public int ServiceCount => MonitorItems.Count(x => x.KindLabel == "后台服务");
    public int FrontendCount => MonitorItems.Count(x => x.KindLabel == "前台插件");

    public MonitorViewModel(IPluginCatalogService pluginCatalog, IPluginLifecycleService pluginLifecycleService)
    {
        _pluginCatalog = pluginCatalog;
        _pluginLifecycleService = pluginLifecycleService;

        SyncPlugins();

        if (_pluginCatalog.Plugins is INotifyCollectionChanged observable)
            observable.CollectionChanged += CatalogPluginsChanged;

        foreach (var plugin in _pluginCatalog.Plugins)
            plugin.PropertyChanged += PluginChanged;
    }

    private void CatalogPluginsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems is not null)
        {
            foreach (PluginEntry plugin in e.OldItems)
                plugin.PropertyChanged -= PluginChanged;
        }

        if (e.NewItems is not null)
        {
            foreach (PluginEntry plugin in e.NewItems)
                plugin.PropertyChanged += PluginChanged;
        }

        SyncPlugins();
    }

    private void PluginChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PluginEntry.Sort)
            or nameof(PluginEntry.IsEnabled)
            or nameof(PluginEntry.LastError)
            or nameof(PluginEntry.State)
            or nameof(PluginEntry.StateChangedAt)
            or nameof(PluginEntry.Name))
        {
            SyncPlugins();
        }
    }

    private void SyncPlugins()
    {
        var selectedMonitorId = SelectedMonitorItem?.Id;

        foreach (var item in MonitorItems)
            item.Dispose();

        MonitorItems.Clear();

        foreach (var plugin in _pluginCatalog.Plugins.OrderBy(x => x.Sort).ThenBy(x => x.Name))
            MonitorItems.Add(new PluginMonitorItemViewModel(plugin, _pluginLifecycleService));

        SelectedMonitorItem = MonitorItems.FirstOrDefault(x => x.Id == selectedMonitorId) ?? MonitorItems.FirstOrDefault();

        OnPropertyChanged(nameof(MonitorItems));
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(RunningCount));
        OnPropertyChanged(nameof(FaultedCount));
        OnPropertyChanged(nameof(DisabledCount));
        OnPropertyChanged(nameof(ServiceCount));
        OnPropertyChanged(nameof(FrontendCount));
    }
}

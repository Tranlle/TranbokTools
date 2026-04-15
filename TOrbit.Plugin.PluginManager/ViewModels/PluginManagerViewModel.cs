using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using TOrbit.Core.Models;
using TOrbit.Core.Services;

namespace TOrbit.Plugin.PluginManager.ViewModels;

public sealed partial class PluginManagerViewModel : ObservableObject
{
    private readonly IPluginCatalogService _pluginCatalog;

    [ObservableProperty]
    private PluginEntry? selectedPlugin;

    public ObservableCollection<PluginEntry> Plugins { get; } = [];

    public string SelectedPluginSortText
    {
        get => (SelectedPlugin?.Sort ?? 0).ToString();
        set
        {
            if (SelectedPlugin is null)
                return;

            if (!int.TryParse(value, out var sort))
                return;

            SelectedPlugin.Sort = Math.Clamp(sort, 0, 100);
            OnPropertyChanged();
        }
    }

    public bool SelectedPluginCanDisable => SelectedPlugin?.CanDisable ?? false;
    public string SelectedPluginBuiltInHint => SelectedPlugin?.BuiltInHint ?? string.Empty;

    public PluginManagerViewModel(IPluginCatalogService pluginCatalog)
    {
        _pluginCatalog = pluginCatalog;

        SyncPlugins();

        if (pluginCatalog.Plugins is INotifyCollectionChanged observable)
            observable.CollectionChanged += CatalogPluginsChanged;

        foreach (var plugin in Plugins)
            plugin.PropertyChanged += PluginChanged;

        SelectedPlugin = Plugins.OrderBy(x => x.Sort).ThenBy(x => x.Name).FirstOrDefault();
    }

    partial void OnSelectedPluginChanged(PluginEntry? value)
    {
        OnPropertyChanged(nameof(SelectedPluginSortText));
        OnPropertyChanged(nameof(SelectedPluginCanDisable));
        OnPropertyChanged(nameof(SelectedPluginBuiltInHint));
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

    private void SyncPlugins()
    {
        var selectedId = SelectedPlugin?.Id;
        Plugins.Clear();

        foreach (var plugin in _pluginCatalog.Plugins.OrderBy(x => x.Sort).ThenBy(x => x.Name))
            Plugins.Add(plugin);

        SelectedPlugin = Plugins.FirstOrDefault(x => x.Id == selectedId)
            ?? Plugins.FirstOrDefault();

        OnPropertyChanged(nameof(Plugins));
    }

    private void PluginChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(PluginEntry.Sort) or nameof(PluginEntry.IsEnabled) or nameof(PluginEntry.Name))
            SyncPlugins();

        if (ReferenceEquals(sender, SelectedPlugin) && e.PropertyName is nameof(PluginEntry.IsEnabled) or nameof(PluginEntry.CanDisable) or nameof(PluginEntry.BuiltInHint))
        {
            OnPropertyChanged(nameof(SelectedPluginCanDisable));
            OnPropertyChanged(nameof(SelectedPluginBuiltInHint));
        }
    }
}

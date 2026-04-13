using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Tranbok.Tools.Designer.Models;

namespace Tranbok.Tools.Designer.Controls;

public partial class PropertyGrid : UserControl
{
    public static readonly StyledProperty<IEnumerable<PropertyGridItem>?> ItemsSourceProperty =
        AvaloniaProperty.Register<PropertyGrid, IEnumerable<PropertyGridItem>?>(nameof(ItemsSource));

    public static readonly StyledProperty<double> LabelColumnWidthProperty =
        AvaloniaProperty.Register<PropertyGrid, double>(nameof(LabelColumnWidth), 130d);

    public PropertyGrid()
    {
        InitializeComponent();
    }

    public IEnumerable<PropertyGridItem>? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public double LabelColumnWidth
    {
        get => GetValue(LabelColumnWidthProperty);
        set => SetValue(LabelColumnWidthProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TOrbit.Designer.Controls;

public partial class ActionGroup : UserControl
{
    public static readonly StyledProperty<object?> BodyProperty =
        AvaloniaProperty.Register<ActionGroup, object?>(nameof(Body));

    public ActionGroup()
    {
        InitializeComponent();
    }

    public object? Body
    {
        get => GetValue(BodyProperty);
        set => SetValue(BodyProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tranbok.Tools.Designer.Controls;

public partial class FormField : UserControl
{
    public static readonly StyledProperty<string?> LabelProperty =
        AvaloniaProperty.Register<FormField, string?>(nameof(Label));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<FormField, string?>(nameof(Description));

    public static readonly StyledProperty<object?> BodyProperty =
        AvaloniaProperty.Register<FormField, object?>(nameof(Body));

    public FormField()
    {
        InitializeComponent();
    }

    public string? Label
    {
        get => GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
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

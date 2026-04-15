using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TOrbit.Designer.Controls;

public partial class EditorHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<EditorHeader, string?>(nameof(Title));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<EditorHeader, object?>(nameof(ActionContent));

    public EditorHeader()
    {
        InitializeComponent();
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

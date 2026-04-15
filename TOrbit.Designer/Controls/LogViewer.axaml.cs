using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TOrbit.Designer.Controls;

public partial class LogViewer : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<LogViewer, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<LogViewer, string?>(nameof(Description));

    public static readonly StyledProperty<string?> TextProperty =
        AvaloniaProperty.Register<LogViewer, string?>(nameof(Text));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<LogViewer, object?>(nameof(ActionContent));

    public LogViewer()
    {
        InitializeComponent();
    }

    public string? Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
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

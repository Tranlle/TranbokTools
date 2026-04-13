using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tranbok.Tools.Designer.Controls;

public partial class PageHeader : UserControl
{
    public static readonly StyledProperty<string?> TitleProperty =
        AvaloniaProperty.Register<PageHeader, string?>(nameof(Title));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<PageHeader, string?>(nameof(Description));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<PageHeader, object?>(nameof(ActionContent));

    public PageHeader()
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

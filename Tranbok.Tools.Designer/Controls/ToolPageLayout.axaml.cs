using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tranbok.Tools.Designer.Controls;

public partial class ToolPageLayout : UserControl
{
    public static readonly StyledProperty<object?> HeaderContentProperty =
        AvaloniaProperty.Register<ToolPageLayout, object?>(nameof(HeaderContent));

    public static readonly StyledProperty<object?> BodyContentProperty =
        AvaloniaProperty.Register<ToolPageLayout, object?>(nameof(BodyContent));

    public static readonly StyledProperty<object?> FooterContentProperty =
        AvaloniaProperty.Register<ToolPageLayout, object?>(nameof(FooterContent));

    public static readonly StyledProperty<GridLength> HeaderBodyGapHeightProperty =
        AvaloniaProperty.Register<ToolPageLayout, GridLength>(nameof(HeaderBodyGapHeight), new GridLength(20));

    public static readonly StyledProperty<GridLength> BodyFooterGapHeightProperty =
        AvaloniaProperty.Register<ToolPageLayout, GridLength>(nameof(BodyFooterGapHeight), new GridLength(20));

    public static readonly StyledProperty<GridLength> FooterHeightProperty =
        AvaloniaProperty.Register<ToolPageLayout, GridLength>(nameof(FooterHeight), new GridLength(180));

    public ToolPageLayout()
    {
        InitializeComponent();
    }

    public object? HeaderContent
    {
        get => GetValue(HeaderContentProperty);
        set => SetValue(HeaderContentProperty, value);
    }

    public object? BodyContent
    {
        get => GetValue(BodyContentProperty);
        set => SetValue(BodyContentProperty, value);
    }

    public object? FooterContent
    {
        get => GetValue(FooterContentProperty);
        set => SetValue(FooterContentProperty, value);
    }

    public GridLength HeaderBodyGapHeight
    {
        get => GetValue(HeaderBodyGapHeightProperty);
        set => SetValue(HeaderBodyGapHeightProperty, value);
    }

    public GridLength BodyFooterGapHeight
    {
        get => GetValue(BodyFooterGapHeightProperty);
        set => SetValue(BodyFooterGapHeightProperty, value);
    }

    public GridLength FooterHeight
    {
        get => GetValue(FooterHeightProperty);
        set => SetValue(FooterHeightProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

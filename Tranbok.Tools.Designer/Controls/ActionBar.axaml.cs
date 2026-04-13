using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Tranbok.Tools.Designer.Controls;

public partial class ActionBar : UserControl
{
    public static readonly StyledProperty<object?> LeadContentProperty =
        AvaloniaProperty.Register<ActionBar, object?>(nameof(LeadContent));

    public static readonly StyledProperty<string?> StatusTextProperty =
        AvaloniaProperty.Register<ActionBar, string?>(nameof(StatusText));

    public static readonly StyledProperty<bool> IsBusyProperty =
        AvaloniaProperty.Register<ActionBar, bool>(nameof(IsBusy));

    public static readonly StyledProperty<object?> ActionsContentProperty =
        AvaloniaProperty.Register<ActionBar, object?>(nameof(ActionsContent));

    public ActionBar()
    {
        InitializeComponent();
    }

    public object? LeadContent
    {
        get => GetValue(LeadContentProperty);
        set => SetValue(LeadContentProperty, value);
    }

    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public bool IsBusy
    {
        get => GetValue(IsBusyProperty);
        set => SetValue(IsBusyProperty, value);
    }

    public object? ActionsContent
    {
        get => GetValue(ActionsContentProperty);
        set => SetValue(ActionsContentProperty, value);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

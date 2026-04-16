using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace TOrbit.Plugin.KeyMap.Views;

public partial class KeyCaptureBox : UserControl
{
    public static readonly StyledProperty<string> ValueProperty =
        AvaloniaProperty.Register<KeyCaptureBox, string>(nameof(Value), string.Empty);

    public string Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private bool _isCapturing;

    // Named controls resolved after XAML load
    private Border? _container;
    private TextBlock? _displayText;
    private TextBlock? _hintText;

    public KeyCaptureBox()
    {
        AvaloniaXamlLoader.Load(this);

        _container = this.FindControl<Border>("CaptureContainer");
        _displayText = this.FindControl<TextBlock>("DisplayText");
        _hintText = this.FindControl<TextBlock>("HintText");

        ValueProperty.Changed.AddClassHandler<KeyCaptureBox>((s, _) => s.UpdateDisplay());
        UpdateDisplay();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        AddHandler(KeyDownEvent, OnCaptureKeyDown, handledEventsToo: false);
        AddHandler(LostFocusEvent, OnLostFocus);
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        RemoveHandler(KeyDownEvent, OnCaptureKeyDown);
        RemoveHandler(LostFocusEvent, OnLostFocus);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (!_isCapturing)
            BeginCapture();

        Focus();
        e.Handled = true;
    }

    private void BeginCapture()
    {
        _isCapturing = true;

        if (_displayText is not null)
            _displayText.Text = "请按下快捷键...";

        if (_hintText is not null)
            _hintText.IsVisible = false;

        if (_container is not null)
        {
            _container.BorderBrush = Avalonia.Application.Current?.FindResource("TOrbitAccentBrush") as Avalonia.Media.IBrush
                ?? _container.BorderBrush;
        }
    }

    private void EndCapture()
    {
        _isCapturing = false;
        UpdateDisplay();

        if (_container is not null)
        {
            _container.ClearValue(Border.BorderBrushProperty);
        }
    }

    private void OnCaptureKeyDown(object? sender, KeyEventArgs e)
    {
        if (!_isCapturing)
            return;

        if (e.Key == Key.Escape)
        {
            EndCapture();
            e.Handled = true;
            return;
        }

        // Ignore lone modifier key presses
        if (e.Key is Key.LeftCtrl or Key.RightCtrl
            or Key.LeftShift or Key.RightShift
            or Key.LeftAlt or Key.RightAlt
            or Key.LWin or Key.RWin)
            return;

        Value = FormatKey(e.Key, e.KeyModifiers);
        EndCapture();
        e.Handled = true;
    }

    private void OnLostFocus(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_isCapturing)
            EndCapture();
    }

    private void UpdateDisplay()
    {
        if (_displayText is null)
            return;

        _displayText.Text = string.IsNullOrWhiteSpace(Value) ? "（未设置）" : Value;

        if (_hintText is not null)
            _hintText.IsVisible = true;
    }

    public static string FormatKey(Key key, KeyModifiers modifiers)
    {
        var parts = new List<string>();

        if (modifiers.HasFlag(KeyModifiers.Control)) parts.Add("Ctrl");
        if (modifiers.HasFlag(KeyModifiers.Alt)) parts.Add("Alt");
        if (modifiers.HasFlag(KeyModifiers.Shift)) parts.Add("Shift");
        if (modifiers.HasFlag(KeyModifiers.Meta)) parts.Add("Meta");

        var keyName = key switch
        {
            Key.OemComma => ",",
            Key.OemPeriod => ".",
            Key.OemSemicolon => ";",
            Key.OemQuotes => "'",
            Key.OemOpenBrackets => "[",
            Key.Oem6 => "]",
            Key.OemBackslash => "\\",
            Key.OemMinus => "-",
            Key.OemPlus => "=",
            Key.Space => "Space",
            Key.Return => "Enter",
            Key.Back => "Backspace",
            Key.Tab => "Tab",
            Key.Delete => "Delete",
            Key.Insert => "Insert",
            Key.Home => "Home",
            Key.End => "End",
            Key.PageUp => "PageUp",
            Key.PageDown => "PageDown",
            Key.Up => "Up",
            Key.Down => "Down",
            Key.Left => "Left",
            Key.Right => "Right",
            _ => key.ToString()
        };

        parts.Add(keyName);
        return string.Join("+", parts);
    }
}

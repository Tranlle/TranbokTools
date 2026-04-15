using Avalonia.Markup.Xaml;

namespace TOrbit.Plugin.KeyMap.Views;

public partial class KeyMapView : Avalonia.Controls.UserControl
{
    public KeyMapView() => AvaloniaXamlLoader.Load(this);
}

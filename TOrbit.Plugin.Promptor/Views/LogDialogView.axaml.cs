using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TOrbit.Plugin.Promptor.Views;

public partial class LogDialogView : UserControl
{
    public LogDialogView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

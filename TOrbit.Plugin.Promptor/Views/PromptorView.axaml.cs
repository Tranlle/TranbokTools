using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace TOrbit.Plugin.Promptor.Views;

public partial class PromptorView : UserControl
{
    public PromptorView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}

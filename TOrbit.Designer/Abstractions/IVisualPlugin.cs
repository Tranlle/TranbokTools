using Avalonia.Controls;

namespace TOrbit.Designer.Abstractions;

public interface IVisualPlugin
{
    Control GetMainView();
}

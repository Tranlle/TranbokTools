using TOrbit.Designer.Models;

namespace TOrbit.Designer.Abstractions;

public interface IThemePaletteProvider
{
    IReadOnlyList<ThemePalette> GetPalettes();
}
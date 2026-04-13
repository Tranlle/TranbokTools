namespace Tranbok.Tools.Designer.ViewModels.Pages;

public sealed class DefaultPluginViewModel(string title, string description)
{
    public string Title { get; } = title;

    public string Description { get; } = description;
}

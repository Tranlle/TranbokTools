using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Tranbok.Tools.Designer.Models;
using Tranbok.Tools.Designer.ViewModels.Dialogs;

namespace Tranbok.Tools.Designer.Services;

public sealed class DesignerDialogService : IDesignerDialogService
{
    public async Task<DesignerDialogResult<bool>> ShowConfirmAsync(Window owner, DesignerConfirmDialogViewModel viewModel)
    {
        var dialog = CreateDialogWindow(owner, viewModel.Title, 520, 280);
        var body = new StackPanel { Spacing = 16 };
        body.Children.Add(new TextBlock
        {
            Text = viewModel.Message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15
        });

        if (!string.IsNullOrWhiteSpace(viewModel.Note))
        {
            body.Children.Add(new TextBlock
            {
                Text = viewModel.Note,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });
        }

        dialog.Content = WrapDialogContent(body, viewModel.ConfirmText, viewModel.CancelText,
            () => dialog.Close(DesignerDialogResult<bool>.Confirmed(true)),
            () => dialog.Close(DesignerDialogResult<bool>.Cancelled(false)));

        var result = await dialog.ShowDialog<DesignerDialogResult<bool>>(owner);
        return result ?? DesignerDialogResult<bool>.Cancelled(false);
    }

    public async Task<DesignerDialogResult<string>> ShowPromptAsync(Window owner, DesignerPromptDialogViewModel viewModel)
    {
        var dialog = CreateDialogWindow(owner, viewModel.Title, 560, 320);
        var input = new TextBox
        {
            PlaceholderText = viewModel.Placeholder,
            Text = viewModel.Value,
            MinWidth = 420
        };

        var body = new StackPanel { Spacing = 16 };
        body.Children.Add(new TextBlock
        {
            Text = viewModel.Message,
            TextWrapping = TextWrapping.Wrap,
            FontSize = 15
        });
        body.Children.Add(input);

        if (!string.IsNullOrWhiteSpace(viewModel.Note))
        {
            body.Children.Add(new TextBlock
            {
                Text = viewModel.Note,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });
        }

        dialog.Content = WrapDialogContent(body, viewModel.ConfirmText, viewModel.CancelText,
            () => dialog.Close(DesignerDialogResult<string>.Confirmed(input.Text?.Trim() ?? string.Empty)),
            () => dialog.Close(DesignerDialogResult<string>.Cancelled()));

        var result = await dialog.ShowDialog<DesignerDialogResult<string>>(owner);
        return result ?? DesignerDialogResult<string>.Cancelled();
    }

    public async Task<DesignerDialogResult<bool>> ShowSheetAsync(Window owner, DesignerSheetViewModel viewModel)
    {
        var dialog = CreateDialogWindow(owner, viewModel.Title, 860, 520);
        var body = new StackPanel { Spacing = 16 };

        if (!string.IsNullOrWhiteSpace(viewModel.Description))
        {
            body.Children.Add(new TextBlock
            {
                Text = viewModel.Description,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.8
            });
        }

        if (viewModel.Content is not null)
            body.Children.Add(viewModel.Content);

        if (!string.IsNullOrWhiteSpace(viewModel.Note))
        {
            body.Children.Add(new TextBlock
            {
                Text = viewModel.Note,
                TextWrapping = TextWrapping.Wrap,
                Opacity = 0.72
            });
        }

        dialog.Content = WrapDialogContent(body, viewModel.ConfirmText, viewModel.CancelText,
            () => dialog.Close(DesignerDialogResult<bool>.Confirmed(true)),
            () => dialog.Close(DesignerDialogResult<bool>.Cancelled(false)));

        var result = await dialog.ShowDialog<DesignerDialogResult<bool>>(owner);
        return result ?? DesignerDialogResult<bool>.Cancelled(false);
    }

    private static Window CreateDialogWindow(Window owner, string title, double width, double height)
    {
        return new Window
        {
            Title = title,
            Width = width,
            Height = height,
            MinWidth = Math.Min(width, 480),
            MinHeight = Math.Min(height, 260),
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = true,
            ShowInTaskbar = false,
            WindowDecorations = WindowDecorations.Full,
            Background = owner.Background
        };
    }

    private static Control WrapDialogContent(Control body, string confirmText, string cancelText, Action onConfirm, Action onCancel)
    {
        var confirmButton = new Button
        {
            Content = confirmText,
            MinWidth = 96
        };
        confirmButton.Click += (_, _) => onConfirm();

        var cancelButton = new Button
        {
            Content = string.IsNullOrWhiteSpace(cancelText) ? "关闭" : cancelText,
            MinWidth = 96
        };
        cancelButton.Click += (_, _) => onCancel();

        var footer = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 12,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        footer.Children.Add(cancelButton);
        footer.Children.Add(confirmButton);

        var grid = new Grid
        {
            Margin = new Thickness(24),
            RowDefinitions = new RowDefinitions("*,Auto"),
            RowSpacing = 20
        };

        grid.Children.Add(body);
        Grid.SetRow(footer, 1);
        grid.Children.Add(footer);
        return grid;
    }
}

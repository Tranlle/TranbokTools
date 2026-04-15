using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using TOrbit.Designer.Models;
using TOrbit.Designer.ViewModels.Dialogs;

namespace TOrbit.Designer.Services;

public interface IDesignerDialogService
{
    Task<DesignerDialogResult<bool>> ShowConfirmAsync(Window owner, DesignerConfirmDialogViewModel viewModel);
    Task<DesignerDialogResult<string>> ShowPromptAsync(Window owner, DesignerPromptDialogViewModel viewModel);
    Task<DesignerDialogResult<bool>> ShowSheetAsync(Window owner, DesignerSheetViewModel viewModel);
    Task<string?> PickFileAsync(Window owner, string title, IReadOnlyList<FilePickerFileType>? fileTypes = null);
    Task<string?> PickFolderAsync(Window owner, string title);
}

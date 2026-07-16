// CoffeeLake (C) 2026-*
// 
// DialogService - manages dialog windows within WorkspaceWindow using FlyoutPresenter
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
// 

using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using SunFlower.Client.View;
using SunFlower.Client.ViewModel;

namespace SunFlower.Client.Service;

public class DialogService(Window mainWindow)
{
    private TaskCompletionSource<object?>? _tcs;

    public async Task<TResult?> ShowDialogAsync<TViewModel, TResult>(TViewModel viewModel)
        where TViewModel : class
    {
        _tcs = new TaskCompletionSource<object?>();
        
        var view = new ViewLocator().Build(viewModel);
        
        if (view is not Control dialogContent) 
            return default;

        // Set DataContext to the view so bindings work
        dialogContent.DataContext = viewModel;

        // Inject DialogService into the ViewModel if it has the property
        if (viewModel is DialogViewModel { DialogService: null } dialogVm)
        {
            dialogVm.DialogService = this;
        }

        var popup = mainWindow.FindControl<Popup>("DialogContainer");
        var dialogLayer = mainWindow.FindControl<Border>("DialogLayer");
        var flyoutPresenter = mainWindow.FindControl<FlyoutPresenter>("DialogPresenter");

        if (popup == null || dialogLayer == null || flyoutPresenter == null)
            return default;

        // Set content to the FlyoutPresenter
        flyoutPresenter.Content = dialogContent;
        
        // Show the dialog
        dialogLayer.IsVisible = true;
        popup.IsOpen = true;

        var result = await _tcs.Task;
        
        // Clean up
        flyoutPresenter.Content = null;
        dialogLayer.IsVisible = false;

        // Clear the reference to avoid memory leaks
        if (viewModel is YesNoDialogViewModel ynVm)
        {
            ynVm.DialogService = null;
        }
        else if (viewModel is DisassemblerDialogViewModel disVm)
        {
            disVm.DialogService = null;
        }
        else if (viewModel is GoToAddressDialogViewModel goVm)
        {
            goVm.DialogService = null;
        }
        
        return result is TResult typedResult 
            ? typedResult 
            : default;
    }

    /// <summary>
    /// Called by the ViewModel to close the dialog with a result.
    /// </summary>
    public void CloseDialog(object? result = null)
    {
        var popup = mainWindow.FindControl<Popup>("DialogContainer");
        var dialogLayer = mainWindow.FindControl<Border>("DialogLayer");
        
        if (popup == null || dialogLayer == null) 
            return;
        
        popup.IsOpen = false;
        dialogLayer.IsVisible = false;
        
        _tcs?.SetResult(result);
    }
}
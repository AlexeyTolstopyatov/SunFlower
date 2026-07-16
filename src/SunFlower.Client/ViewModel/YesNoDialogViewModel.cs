// CoffeeLake (C) 2026-*
// 
// YesNoDialogViewModel - confirmation dialog with Yes/No buttons
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
// 
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SunFlower.Client.ViewModel;

public partial class YesNoDialogViewModel : DialogViewModel
{
    public YesNoDialogViewModel(string title, string text)
    {
        Title = title;
        Message = text;
        ConfirmButtonText = "Yes";
        CancelButtonText = "No";
    }

    [ObservableProperty]
    private string _title = "Confirmation";

    [ObservableProperty]
    private string _message = "Are you sure?";

    [ObservableProperty]
    private string _confirmButtonText = "Yes";

    [ObservableProperty]
    private string _cancelButtonText = "No";
    
    [RelayCommand]
    private void Confirm()
    {
        DialogService?.CloseDialog(true);
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogService?.CloseDialog(false);
    }
}
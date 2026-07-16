// CoffeeLake (C) 2026-*
// 
// GoToAddressDialogViewModel - ViewModel for "Go to Address" dialog window.
// Allows navigating to a specific byte offset in HexEditor.
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace SunFlower.Client.ViewModel;

public partial class GoToAddressDialogViewModel : DialogViewModel
{
    private readonly ulong _maxAddress;

    public GoToAddressDialogViewModel(ulong maxAddress)
    {
        _maxAddress = maxAddress;
    }

    [ObservableProperty]
    private string _addressInput = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public ulong MaxAddress => _maxAddress;

    [RelayCommand]
    private void Cancel()
    {
        DialogService?.CloseDialog(null);
    }

    [RelayCommand]
    private void Accept()
    {
        ErrorMessage = string.Empty;
        
        if (string.IsNullOrWhiteSpace(AddressInput))
        {
            ErrorMessage = "Expected address";
            return;
        }

        if (!ulong.TryParse(AddressInput.Trim(), System.Globalization.NumberStyles.HexNumber, null, out var address))
        {
            ErrorMessage = "Incorrect format (expected: 1A2B)";
            return;
        }

        if (address >= MaxAddress)
        {
            ErrorMessage = $"Out of file bounds (max: {MaxAddress - 1:X})";
            return;
        }

        DialogService?.CloseDialog((ulong?)address);
    }
}
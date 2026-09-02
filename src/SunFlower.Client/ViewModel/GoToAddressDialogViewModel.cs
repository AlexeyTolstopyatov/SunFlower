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

public enum OffsetBase
{
    Hexadecimal,
    Decimal,
    Octal
}

public partial class GoToAddressDialogViewModel(ulong maxAddress) : DialogViewModel
{
    [ObservableProperty]
    private string _addressInput = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private OffsetBase _offsetBase;
    
    [ObservableProperty]
    private bool _isRelativeOffset;

    public ulong MaxAddress { get; } = maxAddress;

    [RelayCommand]
    private void Cancel()
    {
        DialogService?.CloseDialog();
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

        var offset = OffsetBase switch
        {
            OffsetBase.Hexadecimal => Convert.ToUInt64(AddressInput, 16),
            OffsetBase.Octal => Convert.ToUInt64(AddressInput, 8),
            _ => Convert.ToUInt64(AddressInput)
        };
        
        if (offset >= MaxAddress)
        {
            ErrorMessage = $"Out of bounds! (max: {MaxAddress - 1:X})";
            return;
        }
        
        DialogService?.CloseDialog((IsRelativeOffset, offset));
    }

    [RelayCommand]
    private void SwitchOffsetMode()
    {
        IsRelativeOffset = !IsRelativeOffset;
    }
}
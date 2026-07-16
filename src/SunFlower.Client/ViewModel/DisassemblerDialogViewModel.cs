// CoffeeLake (C) 2026-*
// 
// The DisassemblerDialogViewModel.cs represents <what?>
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using SunFlower.Client.Service;


// CoffeeLake (C) 2026-*
// 
// DisassemblerDialogViewModel - ViewModel for disassembly dialog window.
// Handles disassembly of bytes with different architectures.
// 
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SunFlower.Client.Service;

namespace SunFlower.Client.ViewModel;

public partial class DisassemblerDialogViewModel : DialogViewModel
{
    private readonly DisassemblingService _disassemblingService;

    public DisassemblerDialogViewModel(byte[]? bytes, int? startOffset, DisassemblingService disassemblingService, DisassemblerArchitecture? initialArchitecture = null)
    {
        _disassemblingService = disassemblingService;
        
        // Initialize properties
        AvailableArchitectures = Enum.GetValues(typeof(DisassemblerArchitecture));
        SelectedArchitecture = initialArchitecture ?? DisassemblerArchitecture.I8086;
        
        // Store input bytes
        InputBytes = bytes;
        StartOffset = startOffset ?? 0;
        
        IsAllBytesMode = startOffset == null;
    }

    [ObservableProperty]
    private DisassemblerArchitecture _selectedArchitecture;

    public Array AvailableArchitectures { get; }

    [ObservableProperty]
    private string _disassemblyResult = string.Empty;

    [ObservableProperty]
    private bool _isDisassemblyInProgress;

    [ObservableProperty]
    private bool _isAllBytesMode;

    public byte[]? InputBytes { get; }
    
    public int StartOffset { get; }
    
    public string SourceInfo => IsAllBytesMode ? "All bytes" : $"Selected bytes (offset: {StartOffset})";

    [RelayCommand]
    private async Task DisassembleAsync()
    {
        if (InputBytes == null || InputBytes.Length == 0)
        {
            DisassemblyResult = "; (no input bytes)";
            return;
        }

        IsDisassemblyInProgress = true;
        
        try
        {
            var result = await Task.Run(() =>
                _disassemblingService.DisassembleRange(InputBytes, StartOffset, SelectedArchitecture));
            
            DisassemblyResult = result;
        }
        catch (Exception ex)
        {
            DisassemblyResult = $"; Error: {ex.Message}";
        }
        finally
        {
            IsDisassemblyInProgress = false;
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        DialogService?.CloseDialog(null);
    }

    [RelayCommand]
    private void Accept()
    {
        DialogService?.CloseDialog(DisassemblyResult);
    }
}
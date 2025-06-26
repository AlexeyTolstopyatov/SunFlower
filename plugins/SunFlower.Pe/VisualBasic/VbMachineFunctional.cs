using System.Data;
using SunFlower.Pe.Models;
using SunFlower.Pe.Services;

namespace SunFlower.Pe.VisualBasic;

/// 
/// CoffeeLake (C) 2025
/// VBGamer45 (C) 2003
///
/// CSV data used here belongs to VB Semi Decompiler
/// by VBGamer45. Licensed under MIT
///
/// This entity combines ways to explore image internals
/// by JellyBins and [Semi VB Decompiler by VBGamer45]
/// Licensed under MIT
/// 
public class VbMachineFunctional : UnsafeManager
{
    public bool HasFound { get; set; }
    public DataTable FoundOrdinals { get; set; } = new();
    
    public VbMachineFunctional(PeImportTableModel model)
    {
        
    }
}
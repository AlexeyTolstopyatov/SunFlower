// CoffeeLake (C) 2026-*
// 
// The BytesDialogViewModel.cs represents a converter of selected bytes array.
// Expecting destination
//      Integers converting         -> LSB/MSB Int (Decimal)
//      Integers converting         -> LSB/MSB Int (Hexadecimal)
//      ASCII converting            -> string
//      WTF (Windows wide encoding) -> string
//      UTF8 converting             -> string
//      ?UTF7 converting?           -> string
//      IBM codepage converting     -> string
// 
// Condition: make it using FlowerReport.SafeString because of it escapes string  
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com

namespace SunFlower.Client.ViewModel;

public class BytesDialogViewModel : DialogViewModel
{

}
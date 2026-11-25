//
// CoffeeLake (C) 2026-*
// This module represents a "Factory Method", (so sad to see it in functional language)
// to provide a full supported functional of this assembly.
//
// @local_machine: atvlg
// @creator: atolstopyatov2017@vk.com
//
namespace Sunflower.Dasm
/// 
/// Can be used by various plugins/extensions to translate
/// bytes segment into list of common descriptors.
/// 
/// Despite the fact that, various disassemblers in this assembly
/// uses various terms and parts of dest architecture specification, 
/// many CPU modules are executing once task and instruction descriptors might be
/// described more general.
/// For example, all known architectures implements jump mechanics to control
/// conditional executing or have power-control instructions (halt or pause). 
/// 
/// Common instruction descriptor was built to use everywhere instead of
/// raw text to comfort flow control or stack pointer analysis.
/// 
/// 
[<Sealed>]
[<Class>]
type FlowerDasmFactory =
    member _.Decode(seg: byte array, mt: CpuModeType) = 0
    
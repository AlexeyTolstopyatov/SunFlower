namespace Sunflower.Dasm.Intel.Core

type CpuMode =
    | Real16           // 8086..286 real mode
    | Protected16      // 286/386 protected, 16-bit
    | Protected32      // 386+ protected, 32-bit
    | Long64           // x86-64 long mode

module CpuMode =
    /// True for 32-bit default operand size.
    [<CompiledName "Use32">]
    let use32 (mode: CpuMode) =
        match mode with
        | Protected32 -> true
        | _ -> false

    /// True for 64-bit long mode.
    [<CompiledName "Use64">]
    let use64 (mode: CpuMode) =
        match mode with
        | Long64 -> true
        | _ -> false

    /// True when 0x0F is a two-byte opcode prefix (Protected16+).
    [<CompiledName "UseWordPrefix">]
    let useWordPrefix (mode: CpuMode) =
        match mode with
        | Protected16 | Protected32 | Long64 -> true
        | _ -> false

    /// True when 0x66 toggles operand size (Protected32+).
    ///
    /// Protected16 models i286 (no 66/67 prefixes);
    /// i386 in 16-bit protected mode is not separately supported.
    [<CompiledName "Has66Prefix">]
    let has66Prefix (mode: CpuMode) =
        match mode with
        | Protected16 | Protected32 | Long64 -> true
        | _ -> false

    /// True when 0x67 toggles address size (Protected32+).
    [<CompiledName "Has67Prefix">]
    let has67Prefix (mode: CpuMode) =
        match mode with
        | Protected16 | Protected32 | Long64 -> true
        | _ -> false

    /// Default operand size in bytes (without OPSIZE/REX.W overrides).
    [<CompiledName "DefaultOperandSize">]
    let defaultOperandSize (mode: CpuMode) : int =
        match mode with
        | Real16 | Protected16 -> 2
        | Protected32 -> 4
        | Long64 -> 4 // REX.W lifts to 8 in individual instructions

    /// Default address size in bits (without ADSIZE override).
    [<CompiledName "DefaultAddressSize">]
    let defaultAddressSize (mode: CpuMode) : int =
        match mode with
        | Real16 | Protected16 -> 2
        | Protected32 -> 4
        | Long64 -> 8

    /// Instruction-pointer mask used for branch target arithmetic.
    [<CompiledName "GetInstructionPtrMask">]
    let getInstructionPtrMask (mode: CpuMode) : int64 =
        match mode with
        | Long64 -> 0xFFFFFFFFFFFFFFFFL
        | Protected32 -> 0xFFFFFFFFL
        | _ -> 0xFFFFL

    /// Map ProcessorMode into CpuMode.
    let fromCpuArchitectureType (mode: CpuArchitectureType) : CpuMode =
        match mode with
        | I8086 | I186 -> Real16
        | I286 -> Protected16
        | I386 | I486 | IPentium -> Protected32
        | AMD64 -> Long64

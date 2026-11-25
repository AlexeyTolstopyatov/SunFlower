namespace Sunflower.Dasm.Intel.Core

type CpuArchitectureType =
    | I8086
    | I186
    | I286
    | I386
    | I486
    | IPentium
    | AMD64

module CpuArchitectureType =
    /// True for 32-bit flat addressing
    /// (i386+ excluding extended architecture modification).
    [<CompiledName "Use32">]
    let use32 mode =
        match mode with
        | I386
        | I486
        | IPentium -> true
        | _ -> false

    /// True for 64-bit long mode.
    [<CompiledName "Use64">]
    let use64 mode =
        match mode with
        | AMD64 -> true
        | _ -> false

    /// True when the byte 0x0F is a two-byte opcode prefix (i286+).
    /// On i8086/i186 0x0F is a valid one-byte POP CS.
    [<CompiledName "UseWordPrefix">]
    let useWordPrefix mode =
        match mode with
        | I286
        | I386
        | I486
        | IPentium
        | AMD64 -> true
        | _ -> false

    /// True when 0x66 toggles operand size (i386+).
    [<CompiledName "Has66Prefix">]
    let has66Prefix mode =
        match mode with
        | I386
        | I486
        | IPentium
        | AMD64 -> true
        | _ -> false

    /// True when 0x67 toggles address size (i386+).
    [<CompiledName "Has67Prefix">]
    let has67Prefix mode =
        match mode with
        | I386
        | I486
        | IPentium
        | AMD64 -> true
        | _ -> false

    /// Instruction-pointer mask used for branch target arithmetic.
    [<CompiledName "GetInstructionPtrMask">]
    let getInstructionPtrMask mode =
        if use64 mode then 0xFFFFFFFFFFFFFFFFL
        elif use32 mode then 0xFFFFFFFFL
        else 0xFFFFL


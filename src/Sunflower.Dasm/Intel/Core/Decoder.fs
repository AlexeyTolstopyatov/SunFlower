//
// CoffeeLake (C) 2026-*
// This module represents a complete IA32(e) decoder which uses
// various external sources to disassemble instruction byte array.
//
// In example: It accepts 2 arguments to map instruction bytes:
//      -> Instruction database to concrete architecture mapping;
//      -> CpuArchitectureType to trim register/memory analysis.
// without any string/sp analysis. If this database file is missing,
// next following instructions are be aborted. (raises failure)
//
// @creator: atolstopyatov2017@vk.com
// @local_machine: atvlg
//
namespace Sunflower.Dasm.Intel.Core

open System
open System.IO
open System.Text
open System.Text.Json

type RepPrefix =
    | Repne   // F2
    | Repz    // F3
    | Rep     // F3 (alias, same byte as Repz)

type SegmentPrefix =
    | Es  // 0x26
    | Cs  // 0x2E
    | Ss  // 0x36
    | Ds  // 0x3E
    | Fs  // 0x64
    | Gs  // 0x65

type RexState =
    { W: bool
      R: bool
      X: bool
      B: bool }

type PrefixState = {
    Opsize: bool      // 0x66
    Adsize: bool      // 0x67
    Lock: bool        // 0xF0
    Rep: RepPrefix option
    Segment: SegmentPrefix option
    Rex: RexState option   // <-- Presents only for Long64; Will set to [None] in 16/32-bit
}

module PrefixHelpers =
    let emptyPrefixState : PrefixState =
        { Opsize = false
          Adsize = false
          Lock = false
          Rep = None
          Segment = None
          Rex = None }

    // Legacy prefix bytes (same for all modes).
    let legacyPrefixBytes =
        [ 0xF0uy;   // LOCK
          0xF2uy;   // REPNE / XACQUIRE
          0xF3uy;   // REPZ / REP / XRELEASE
          0x26uy;   // ES
          0x2Euy;   // CS
          0x36uy;   // SS
          0x3Euy;   // DS
          0x64uy;   // FS
          0x65uy;   // GS
          0x66uy;   // OPSIZE
          0x67uy ]  // ADSIZE

    let isLegacyPrefix (b: byte) = List.contains b legacyPrefixBytes

    let segmentOfByte (b: byte) : SegmentPrefix option =
        match b with
        | 0x26uy -> Some Es
        | 0x2Euy -> Some Cs
        | 0x36uy -> Some Ss
        | 0x3Euy -> Some Ds
        | 0x64uy -> Some Fs
        | 0x65uy -> Some Gs
        | _      -> None

    let repOfByte (b: byte) : RepPrefix option =
        match b with
        | 0xF2uy -> Some Repne
        | 0xF3uy -> Some Repz
        | _      -> None

    // Build RexState from a REX byte (0x40-0x4F).
    let rexOfByte (b: byte) : RexState option =
        if b >= 0x40uy && b <= 0x4Fuy then
            Some { W = (b &&& 0x08uy) <> 0x0uy
                   R = (b &&& 0x04uy) <> 0x0uy
                   X = (b &&& 0x02uy) <> 0x0uy
                   B = (b &&& 0x01uy) <> 0x0uy }
        else None

type DecoderState =
    { opcodesMap: Map<string, Operation>
      prefixSet: Set<byte>
      interrupts: Map<byte, FlowControlDescriptor>
      path: string
      mode: CpuArchitectureType
      cpuMode: CpuMode }

    member this.Is64Bit = CpuMode.use64 this.cpuMode
    member this.Is32Bit = CpuMode.use32 this.cpuMode

type Instruction =
    { Opcode: string
      Reg: Nullable<int>
      Mnemonic: string
      Operands: List<string>
      ModRM: bool
      IsPrefix: bool }

type DisassembledInstruction =
    { Offset: int
      Length: int
      Mnemonic: string
      Bytes: byte array }

type FlowControl =
    | FallThrough
    | Jump
    | ConditionalJump
    | Call
    | Return
    | IndirectJump
    | IndirectCall
    | IndirectJumpFar
    | IndirectCallFar
    | SoftwareInterrupt
    | Terminate
    | Halt

type ByteStatus =
    | Unknown = 0
    | Code = 1
    | Data = 2

/// Structured representation of a single operand.
/// Address-bearing operands stay in instruction-relative form so that
/// `formatOperandPart` can add `instrOffset` and apply the correct IP mask
/// for the current processor mode.
type OperandPart =
    | Reg of string
    | Imm of uint64 * int // value, size in bytes
    | SignedImm of int64 * int // Ibs/Jbs: sign-extended byte
    | Rel of int * int // instr-relative target, size
    | Abs of uint64 * int // [absolute]
    | Mem of string // already fully formatted (has brackets)
    | Far of uint16 * uint64 // selector, offset
    | Raw of string // fallback

/// Where in the instruction an operand's bytes live.
type DecodedOperand =
    { Part: OperandPart
      BytesOffset: int
      BytesLength: int }

/// Fully decoded instruction *before* text formatting.
type DecodedPart =
    { Mnemonic: string
      RawMnemonic: string
      Operands: DecodedOperand list
      Length: int }

type DecodedInstruction =
    { Offset: int
      Length: int
      Mnemonic: string
      RawMnemonic: string
      Operands: DecodedOperand[]
      Bytes: byte array
      Flow: FlowControl
      Targets: int list
      Comment: string option }

module public Decoder =
    let private gpr8List =
        [| "AL"; "CL"; "DL"; "BL"; "AH"; "CH"; "DH"; "BH" |]

    let private gpr8RexList =
        [| "AL"; "CL"; "DL"; "BL"; "SPL"; "BPL"; "SIL"; "DIL"
           "R8L"; "R9L"; "R10L"; "R11L"; "R12L"; "R13L"; "R14L"; "R15L" |]

    let private gpr16List =
        [| "AX"; "CX"; "DX"; "BX"; "SP"; "BP"; "SI"; "DI" |]

    let private gpr16RexList =
        [| "AX"; "CX"; "DX"; "BX"; "SP"; "BP"; "SI"; "DI"
           "R8W"; "R9W"; "R10W"; "R11W"; "R12W"; "R13W"; "R14W"; "R15W" |]

    let private gpr32List =
        [| "EAX"; "ECX"; "EDX"; "EBX"; "ESP"; "EBP"; "ESI"; "EDI" |]

    let private gpr32RexList =
        [| "EAX"; "ECX"; "EDX"; "EBX"; "ESP"; "EBP"; "ESI"; "EDI"
           "R8D"; "R9D"; "R10D"; "R11D"; "R12D"; "R13D"; "R14D"; "R15D" |]

    let private gpr64RexList =
        [| "RAX"; "RCX"; "RDX"; "RBX"; "RSP"; "RBP"; "RSI"; "RDI"
           "R8"; "R9"; "R10"; "R11"; "R12"; "R13"; "R14"; "R15" |]

    let private srList = [| "ES"; "CS"; "SS"; "DS"; "FS"; "GS" |]
    let private crList = [| "CR0"; "CR1"; "CR2"; "CR3"; "CR4"; "CR5"; "CR6"; "CR7" |]
    let private drList = [| "DR0"; "DR1"; "DR2"; "DR3"; "DR4"; "DR5"; "DR6"; "DR7" |]

    let private tryReadByte (i: int) (bytes: byte[]) =
        if i >= 0 && i < bytes.Length then Some(i + 1, bytes[i])
        else None

    let private tryReadBytes (n: int) (i: int) (bytes: byte[]) =
        if n > 0 && i >= 0 && i + n <= bytes.Length then
            Some(i + n, bytes[i .. i + n - 1])
        else
            None

    let private formatImmValue (v: uint64) (size: int) =
        match size with
        | 1 -> $"0x%02X{v &&& 0xFFUL}"
        | 2 -> $"0x%04X{v &&& 0xFFFFUL}"
        | 4 -> $"0x%08X{v &&& 0xFFFFFFFFUL}"
        | 8 -> $"0x%016X{v}"
        | _ -> $"0x%X{v}"

    let private dispStr (d: int64) =
        if d >= 0L then $"0x%X{d}" else $"-0x%X{-d}"

    let private joinMemParts (parts: string list) =
        let filtered = parts |> List.filter ((<>) "")
        if filtered.IsEmpty then "[0x0]"
        else
            let s = String.Join("+", filtered)
            sprintf "[%s]" (s.Replace("+-", "-"))

    //==================================================================
    // Immediate size resolution.
    //
    // Critical for x86-64
    //   - Iz: always imm32 in long mode, even with REX.W=1
    //         (exception may be a PUSH <imm32>, which also uses <imm32> but is sign-extended
    //          to 64 by the CPU; we still report 4 here).
    //   - Jz: rel32 in long mode, regardless of REX.W.
    //   - Iv: operand-sized -> 8 bytes when REX.W=1 in long mode.
    //   - Ob/Ov: moffs -> absolute address, so sized by *addressSize*.
    //==================================================================
    let private immediateSize (token: string) (opSize: int) (addressSize: int) =
        match token with
        | "Ib" | "Ibs" | "Jb" | "Jbs" -> 1
        | "Iw" -> 2
        | "Id" -> 4
        | "Iz" -> min opSize 4
        | "Jz" -> if opSize = 8 then 4 else opSize
        | "Iv" -> opSize
        | "Ob" | "Ov" ->
            if addressSize = 64 then 8
            elif addressSize = 32 then 4
            else 2
        | "Mp" | "Ap" ->
            if opSize = 8 then 10
            elif opSize = 4 then 6
            else 4
        | t -> failwithf $"Unsupported immediate token: %s{t}"

    let private isImmediateToken (token: string) =
        match token with
        | "Ib" | "Ibs" | "Iv" | "Iz" | "Id" | "Iw"
        | "Jb" | "Jbs" | "Jz"
        | "Ob" | "Ov" | "Mp" | "Ap" -> true
        | _ -> false

    /// Determine operand size in bytes from REX.W and 0x66 prefix.
    /// (mode-agnostic version used by classifyFlow; the opcode-aware
    /// variant lives in opSizeOf.)
    let private operandSize (mode: CpuMode) (rexW: bool) (has66: bool) =
        if CpuMode.use64 mode && rexW then 8
        elif has66 then 2
        elif CpuMode.use64 mode then 4
        else if CpuMode.use32 mode then 4
        else 2

    /// Apply the operand size of the concrete opcode. In long mode,
    /// stack-relative and transfer-control instructions default to
    /// 64-bit operand size, not the generic 32-bit default.
    let private opSizeOf (state: DecoderState) (op: Operation) (prefix: PrefixState) (rex: RexState option) =
        let has66 = prefix.Opsize && CpuMode.has66Prefix state.cpuMode
        let rexW =
            match rex with
            | Some r -> r.W
            | None -> false
        
        let isLong64StackDefault =
            CpuMode.use64 state.cpuMode
            && not has66
            && (op.mnemonic = "PUSH" || op.mnemonic = "POP"
                || op.mnemonic = "CALL" || op.mnemonic = "JMP"
                || op.mnemonic = "CALLF" || op.mnemonic = "JMPF"
                || op.mnemonic = "RETN" || op.mnemonic = "RETF"
                || op.mnemonic = "LEAVE" || op.mnemonic = "ENTER")

        if isLong64StackDefault then 8
        elif CpuMode.use64 state.cpuMode && rexW then 8
        elif has66 then 2
        elif CpuMode.use64 state.cpuMode then 4
        else if CpuMode.use32 state.cpuMode then 4
        else 2

    let private expandRegToken (token: string) (opSize: int) (_: CpuMode) =
        let pick r8 r32 r16 =
            if opSize = 8 then r8
            elif opSize = 4 then r32
            else r16
        
        match token with
        | "rAX" | "eAX" -> pick "RAX" "EAX" "AX"
        | "rCX" | "eCX" -> pick "RCX" "ECX" "CX"
        | "rDX" | "eDX" -> pick "RDX" "EDX" "DX"
        | "rBX" | "eBX" -> pick "RBX" "EBX" "BX"
        | "rSP" | "eSP" -> pick "RSP" "ESP" "SP"
        | "rBP" | "eBP" -> pick "RBP" "EBP" "BP"
        | "rSI" | "eSI" -> pick "RSI" "ESI" "SI"
        | "rDI" | "eDI" -> pick "RDI" "EDI" "DI"
        // Aliases used by the 386 JSON (register-in-opcode forms). These
        // are placeholders when no REX.B info is available; the correct
        // REX.B extension is applied in resolveNoModRMOperands.
        | "rAX/r8" | "eAX/r8" -> pick "RAX" "EAX" "AX"
        | "rCX/r9" | "eCX/r9" -> pick "RCX" "ECX" "CX"
        | "rDX/r10" | "eDX/r10" -> pick "RDX" "EDX" "DX"
        | "rBX/r11" | "eBX/r11" -> pick "RBX" "EBX" "BX"
        | "rSP/r12" | "eSP/r12" -> pick "RSP" "ESP" "SP"
        | "rBP/r13" | "eBP/r13" -> pick "RBP" "EBP" "BP"
        | "rSI/r14" | "eSI/r14" -> pick "RSI" "ESI" "SI"
        | "rDI/r15" | "eDI/r15" -> pick "RDI" "EDI" "DI"
        | "AL/R8L" -> "AL"
        | "CL/R9L" -> "CL"
        | "DL/R10L" -> "DL"
        | "BL/R11L" -> "BL"
        | "AH/R12L" -> "AH"
        | "CH/R13L" -> "CH"
        | "DH/R14L" -> "DH"
        | "BH/R15L" -> "BH"
        | t -> t

    /// 16-bit effective address. `idx` points at the byte after the ModRM.
    let private decodeMem16 (rm: byte) (modBits: byte) (idx: int) (bytes: byte[]) : (string * int) option =
        if modBits = 0uy && rm = 6uy then
            tryReadBytes 2 idx bytes
            |> Option.map (fun (ni, bs) -> $"[0x%04X{BitConverter.ToUInt16(bs, 0)}]", ni)
        else
            let basePart =
                match rm with
                | 0uy -> "BX+SI"
                | 1uy -> "BX+DI"
                | 2uy -> "BP+SI"
                | 3uy -> "BP+DI"
                | 4uy -> "SI"
                | 5uy -> "DI"
                | 6uy -> "BP"
                | 7uy -> "BX"
                | _ -> "?"

            let dispResult =
                match modBits with
                | 1uy -> tryReadByte idx bytes |> Option.map (fun (ni, b) -> int64 (sbyte b), ni)
                | 2uy ->
                    tryReadBytes 2 idx bytes
                    |> Option.map (fun (ni, bs) -> int64 (BitConverter.ToInt16(bs, 0)), ni)
                | _ -> Some(0L, idx)

            match dispResult with
            | None -> None
            | Some(d, ni) ->
                let text =
                    if d = 0L then $"[%s{basePart}]"
                    else $"[%s{basePart}+%s{dispStr d}]" |> _.Replace("+-", "-")

                Some(text, ni)

    /// 32/64-bit effective address including SIB and RIP-relative.
    ///
    /// `addressSize` determines register naming and the meaning of
    /// `rm=5 mod=0`:
    ///   - addressSize = 64  → RIP-relative, table = gpr64RexList,
    ///                          REX.R/X/B honored.
    ///   - addressSize = 32  → absolute [disp32], table = gpr32List,
    ///                          REX.* for addressing ignored.
    let private decodeMem32
        (rm: byte)
        (modBits: byte)
        (idx: int)
        (bytes: byte[])
        (rex: RexState option)
        (mode: CpuMode)
        (addressSize: int)
        : (string * int) option =
        let tryDisp8 () =
            tryReadByte idx bytes |> Option.map (fun (ni, b) -> int64 (sbyte b), ni)

        let tryDisp32 () =
            tryReadBytes 4 idx bytes
            |> Option.map (fun (ni, bs) -> int64 (BitConverter.ToInt32(bs, 0)), ni)

        let baseTable =
            if addressSize = 64 then gpr64RexList
            elif addressSize = 32 then gpr32List
            else gpr16List   // defensive; decodeMem32 is not called in 16-bit

        let rexB = if addressSize = 64 then (match rex with Some r -> r.B | None -> false) else false
        let rexX = if addressSize = 64 then (match rex with Some r -> r.X | None -> false) else false

        if rm = 4uy then
            match tryReadByte idx bytes with
            | None -> None
            | Some(afterSib, sib) ->
                let scale = (sib >>> 6) &&& 0b11uy
                let indexReg = (sib >>> 3) &&& 0b111uy
                let baseReg = sib &&& 0b111uy
                let hasNoBase = baseReg = 5uy && modBits = 0uy

                let baseName =
                    if hasNoBase then ""
                    else
                        let idx' = int baseReg + (if rexB then 8 else 0)
                        if idx' < baseTable.Length then baseTable[idx'] else baseTable[int baseReg]

                let indexPart =
                    if indexReg = 4uy && not rexX then ""
                    else
                        let idx' = int indexReg + (if rexX then 8 else 0)
                        if idx' < baseTable.Length then
                            let n = baseTable[idx']
                            if scale = 0uy then n else $"%s{n}*%d{1 <<< int scale}"
                        else ""

                let dispResult =
                    match modBits with
                    | 0uy when hasNoBase -> tryDisp32 ()
                    | 1uy -> tryDisp8 ()
                    | 2uy -> tryDisp32 ()
                    | _ -> Some(0L, afterSib)

                match dispResult with
                | None -> None
                | Some(d, finalIdx) ->
                    let showDisp = d <> 0L || hasNoBase
                    let dispPart = if showDisp then dispStr d else ""
                    Some(joinMemParts [ baseName; indexPart; dispPart ], finalIdx)
        else
            let regIdx = int rm + (if rexB then 8 else 0)

            if modBits = 0uy && rm = 5uy && addressSize = 64 then
                // RIP-relative uses only in 64-bit addressing mode.
                match tryDisp32 () with
                | None -> None
                | Some(d, finalIdx) -> Some($"[RIP+%s{dispStr d}]", finalIdx)
            else
                let baseName =
                    if regIdx < baseTable.Length then baseTable[regIdx]
                    else baseTable[int rm]

                let dispResult =
                    match modBits with
                    | 0uy when rm = 5uy -> tryDisp32 ()
                    | 1uy -> tryDisp8 ()
                    | 2uy -> tryDisp32 ()
                    | _ -> Some(0L, idx)

                match dispResult with
                | None -> None
                | Some(d, finalIdx) ->
                    let text =
                        match modBits with
                        | 0uy when rm = 5uy -> $"[0x%X{d}]"
                        | 0uy -> $"[%s{baseName}]"
                        | _ ->
                            if d = 0L then $"[%s{baseName}]"
                            else $"[%s{baseName}+%s{dispStr d}]" |> _.Replace("+-", "-")

                    Some(text, finalIdx)

    /// No-ModRM operand resolver. Handles the "register in opcode" forms
    /// (PUSH/POP r8-r15, MOV r8, imm64, MOV AL/R8L imm8, etc.).
    ///
    /// `rex` is required so that:
    ///   41 50          -> PUSH R8
    ///   B4 imm8        -> MOV AH,  imm8
    ///   40 B4 imm8     -> MOV SPL, imm8
    ///   41 B4 imm8     -> MOV R12L, imm8
    /// are all distinguished correctly.
    let private resolveNoModRMOperands
        (mode: CpuMode)
        (opSize: int)
        (addressSize: int)
        (bytes: byte[])
        (startIdx: int)
        (operands: string list)
        (rex: RexState option)
        : (int * DecodedOperand list) option =

        let reg (name: string) : DecodedOperand =
            { Part = Reg name; BytesOffset = -1; BytesLength = 0 }

        let raw (text: string) : DecodedOperand =
            { Part = Raw text; BytesOffset = -1; BytesLength = 0 }

        // GPR from the low bits of the opcode byte, extended by REX.B.
        let gprFromOpcode (baseIdx: int) : string =
            let hasRex = rex.IsSome && CpuMode.use64 mode
            let rexB = match rex with Some r -> r.B | None -> false
            let idx = baseIdx + (if rexB then 8 else 0)
            let table =
                if opSize = 8 then gpr64RexList
                elif opSize = 4 then (if hasRex then gpr32RexList else gpr32List)
                else (if hasRex then gpr16RexList else gpr16List)
            if idx < table.Length then table[idx] else table[idx % table.Length]

        // 8-bit GPR from the opcode byte. Any REX prefix remaps AH/CH/DH/BH
        // to SPL/BPL/SIL/DIL; REX.B additionally enables R8L..R15L.
        let gpr8FromOpcode (baseIdx: int) : string =
            let hasRex = rex.IsSome && CpuMode.use64 mode
            let rexB = match rex with Some r -> r.B | None -> false
            let idx = baseIdx + (if rexB then 8 else 0)
            let table = if hasRex then gpr8RexList else gpr8List
            if idx < table.Length then table[idx] else table[idx % table.Length]

        let rec loop idx tokens acc =
            match tokens with
            | [] -> Some(idx, List.rev acc)
            | token :: rest ->
                match token with

                // Fixed 8-bit / 16-bit registers.
                | "AL" | "CL" | "DL" | "BL" | "AH" | "CH" | "DH" | "BH"
                | "ES" | "CS" | "SS" | "DS" | "FS" | "GS"
                | "DX" ->
                    loop idx rest (reg token :: acc)

                // Register-in-opcode (PUSH/POP r8-r15, XCHG r, RAX, etc.).
                | "rAX/r8" | "eAX/r8" -> loop idx rest (reg (gprFromOpcode 0) :: acc)
                | "rCX/r9" | "eCX/r9" -> loop idx rest (reg (gprFromOpcode 1) :: acc)
                | "rDX/r10" | "eDX/r10" -> loop idx rest (reg (gprFromOpcode 2) :: acc)
                | "rBX/r11" | "eBX/r11" -> loop idx rest (reg (gprFromOpcode 3) :: acc)
                | "rSP/r12" | "eSP/r12" -> loop idx rest (reg (gprFromOpcode 4) :: acc)
                | "rBP/r13" | "eBP/r13" -> loop idx rest (reg (gprFromOpcode 5) :: acc)
                | "rSI/r14" | "eSI/r14" -> loop idx rest (reg (gprFromOpcode 6) :: acc)
                | "rDI/r15" | "eDI/r15" -> loop idx rest (reg (gprFromOpcode 7) :: acc)

                | "AL/R8L" -> loop idx rest (reg (gpr8FromOpcode 0) :: acc)
                | "CL/R9L" -> loop idx rest (reg (gpr8FromOpcode 1) :: acc)
                | "DL/R10L" -> loop idx rest (reg (gpr8FromOpcode 2) :: acc)
                | "BL/R11L" -> loop idx rest (reg (gpr8FromOpcode 3) :: acc)
                | "AH/R12L" -> loop idx rest (reg (gpr8FromOpcode 4) :: acc)
                | "CH/R13L" -> loop idx rest (reg (gpr8FromOpcode 5) :: acc)
                | "DH/R14L" -> loop idx rest (reg (gpr8FromOpcode 6) :: acc)
                | "BH/R15L" -> loop idx rest (reg (gpr8FromOpcode 7) :: acc)

                // opSize-sized accumulator forms (ADD rAX, Iz etc.) -> no REX.B
                // applies here, only opSize + 0x66.
                | "rAX" | "rCX" | "rDX" | "rBX" | "rSP" | "rBP" | "rSI" | "rDI"
                | "eAX" | "eCX" | "eDX" | "eBX" | "eSP" | "eBP" | "eSI" | "eDI" ->
                    loop idx rest (reg (expandRegToken token opSize mode) :: acc)

                | "Ib" ->
                    match tryReadByte idx bytes with
                    | None -> None
                    | Some(ni, b) ->
                        loop ni rest
                            ({ Part = Imm(uint64 b, 1); BytesOffset = idx; BytesLength = 1 } :: acc)

                | "Ibs" ->
                    match tryReadByte idx bytes with
                    | None -> None
                    | Some(ni, b) ->
                        loop ni rest
                            ({ Part = SignedImm(int64 (sbyte b), 1); BytesOffset = idx; BytesLength = 1 } :: acc)

                | "Iw" ->
                    match tryReadBytes 2 idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        loop ni rest
                            ({ Part = Imm(uint64 (BitConverter.ToUInt16(bs, 0)), 2)
                               BytesOffset = idx; BytesLength = 2 } :: acc)

                | "Id" ->
                    match tryReadBytes 4 idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        loop ni rest
                            ({ Part = Imm(uint64 (BitConverter.ToUInt32(bs, 0)), 4)
                               BytesOffset = idx; BytesLength = 4 } :: acc)

                | "Iz" | "Iv" ->
                    let n = immediateSize token opSize addressSize
                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        let v =
                            if n = 8 then BitConverter.ToUInt64(bs, 0)
                            elif n = 4 then uint64 (BitConverter.ToUInt32(bs, 0))
                            else uint64 (BitConverter.ToUInt16(bs, 0))
                        loop ni rest
                            ({ Part = Imm(v, n); BytesOffset = idx; BytesLength = n } :: acc)

                | "Jb" | "Jbs" ->
                    match tryReadByte idx bytes with
                    | None -> None
                    | Some(ni, rel) ->
                        let d = int (sbyte rel)
                        loop ni rest
                            ({ Part = Rel(ni + d, 1); BytesOffset = idx; BytesLength = 1 } :: acc)

                | "Jz" ->
                    let n = immediateSize token opSize addressSize
                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        let d =
                            if n = 4 then int (BitConverter.ToInt32(bs, 0))
                            else int (BitConverter.ToInt16(bs, 0))
                        loop ni rest
                            ({ Part = Rel(ni + d, n); BytesOffset = idx; BytesLength = n } :: acc)

                | "Ap" | "Mp" ->
                    let n = immediateSize token opSize addressSize
                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        let seg, off =
                            if n = 10 then BitConverter.ToUInt16(bs, 8), BitConverter.ToUInt64(bs, 0)
                            elif n = 6 then BitConverter.ToUInt16(bs, 4), uint64 (BitConverter.ToUInt32(bs, 0))
                            else BitConverter.ToUInt16(bs, 2), uint64 (BitConverter.ToUInt16(bs, 0))
                        loop ni rest
                            ({ Part = Far(seg, off); BytesOffset = idx; BytesLength = n } :: acc)

                | "Ob" | "Ov" ->
                    let n =
                        if addressSize = 64 then 8
                        elif addressSize = 32 then 4
                        else 2
                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        let v =
                            if n = 8 then BitConverter.ToUInt64(bs, 0)
                            elif n = 4 then uint64 (BitConverter.ToUInt32(bs, 0))
                            else uint64 (BitConverter.ToUInt16(bs, 0))
                        loop ni rest
                            ({ Part = Abs(v, n); BytesOffset = idx; BytesLength = n } :: acc)

                | "Xb" | "Yb" | "Xv" | "Yv" | "Fv" -> loop idx rest (raw "" :: acc)

                | t when t.Contains('/') ->
                    let simple = t.Split('/').[0].Trim()
                    loop idx rest (reg (expandRegToken simple opSize mode) :: acc)

                | t -> loop idx rest (raw t :: acc)

        loop startIdx operands []

    let private resolveModRMOperands
        (opcodesMap: Map<string, Operation>)
        (opcodeKey: string)
        (defaultOp: Operation)
        (mode: CpuMode)
        (opSize: int)
        (addressSize: int)
        (bytes: byte[])
        (startIdx: int)
        (rex: RexState option)
        (segPrefix: SegmentPrefix option)
        : (int * string * DecodedOperand list) option =

        match tryReadByte startIdx bytes with
        | None -> None
        | Some(idxAfterModRm, modrm) ->
            let modBits = (modrm >>> 6) &&& 0b11uy
            let registerBit = (modrm >>> 3) &&& 0b111uy
            let rmBit = modrm &&& 0b111uy

            let rexR = match rex with Some r -> r.R | None -> false
            let rexB = match rex with Some r -> r.B | None -> false

            let registerBitExt = int registerBit + (if rexR then 8 else 0)
            let rmBitExt = int rmBit + (if rexB then 8 else 0)

            let findGroup (suffix: string) =
                let key =
                    if suffix = "" then $"%s{opcodeKey}/%d{int registerBit}"
                    else $"%s{opcodeKey}/%s{suffix}"
                Map.tryFind key opcodesMap

            let tryRegisterDirect () =
                match findGroup $"+%d{int registerBit}p" with
                | Some op -> Some op
                | None -> findGroup $"x%d{int registerBit}p"

            let isFpuEscape =
                opcodeKey.Length >= 2 && opcodeKey[0] = 'D' && opcodeKey[1] >= '8' && opcodeKey[1] <= 'F'

            let fitsRegisterForm (op: Operation) =
                not isFpuEscape || op.operands |> List.forall (fun t -> not (t.StartsWith("M")))

            let effectiveOperation =
                if modBits = 3uy then
                    match tryRegisterDirect () with
                    | Some g -> g
                    | None ->
                        match findGroup "" with
                        | Some g when fitsRegisterForm g -> g
                        | _ -> defaultOp
                else
                    match findGroup "" with
                    | Some g -> g
                    | None -> defaultOp

            let operands = effectiveOperation.operands
            if operands.IsEmpty then None
            else

            // Per-token register table. Fixes MOVZX/MOVSX/MOVSXD where
            // the two operands have *different* widths (e.g. Gv, Eb).
            let registerTableFor (token: string) : string[] =
                let suffix =
                    if token.Length > 1 && not (token.StartsWith("ST")) then token.Substring(1)
                    else ""
                if suffix = "b" then
                    if CpuMode.use64 mode && rex.IsSome then gpr8RexList else gpr8List
                elif suffix = "w" then
                    if CpuMode.use64 mode && rex.IsSome then gpr16RexList else gpr16List
                elif suffix = "d" then
                    if CpuMode.use64 mode && rex.IsSome then gpr32RexList else gpr32List
                elif suffix = "q" then gpr64RexList
                elif opSize = 8 then gpr64RexList
                elif opSize = 4 then
                    if CpuMode.use64 mode && rex.IsSome then gpr32RexList else gpr32List
                else
                    if CpuMode.use64 mode && rex.IsSome then gpr16RexList else gpr16List

            let registerNameFor (token: string) (idx: int) : string =
                let table = registerTableFor token
                if idx < table.Length then table[idx]
                else table[idx % table.Length]

            let tryGetSegmentRegisterName (r: byte) : string option =
                let idx = int r
                // В 16/32-битном защищённом режиме и выше MOV CS, ... (8E /1) также является #UD
                if idx < 0 || idx >= srList.Length then None
                elif opcodeKey = "8E" && idx = 1 && mode <> Real16 then None
                else Some srList[idx]

            // r/m effective text. For mod=3 the actual register name depends
            // on the concrete E-token and is produced inside mkRm.
            let rmResult: (string * int) option =
                if modBits = 3uy then Some("", idxAfterModRm)
                elif addressSize = 64 then
                    decodeMem32 rmBit modBits idxAfterModRm bytes rex mode addressSize
                elif addressSize = 32 then
                    decodeMem32 rmBit modBits idxAfterModRm bytes rex mode addressSize
                else
                    decodeMem16 rmBit modBits idxAfterModRm bytes

            match rmResult with
            | None -> None
            | Some(rmText, idxAfterRm) ->
                let isSibCase = addressSize >= 32 && rmBit = 4uy && modBits <> 3uy
                let dispStart = if isSibCase then idxAfterModRm + 1 else idxAfterModRm
                let dispLen = max 0 (idxAfterRm - dispStart)

                let segName =
                    match segPrefix with
                    | Some Es -> "ES" | Some Cs -> "CS" | Some Ss -> "SS"
                    | Some Ds -> "DS" | Some Fs -> "FS" | Some Gs -> "GS"
                    | None -> ""

                let mkRm (token: string) : DecodedOperand =
                    if modBits = 3uy then
                        { Part = Reg (registerNameFor token rmBitExt); BytesOffset = -1; BytesLength = 0 }
                    else
                        let mem = if segName = "" then rmText else $"%s{segName}:%s{rmText}"
                        { Part = Mem mem
                          BytesOffset = if dispLen > 0 then dispStart else -1
                          BytesLength = dispLen }

                let reg (name: string) : DecodedOperand =
                    { Part = Reg name; BytesOffset = -1; BytesLength = 0 }

                let raw (text: string) : DecodedOperand =
                    { Part = Raw text; BytesOffset = -1; BytesLength = 0 }

                let mutable idx = idxAfterRm

                let rec loop tokens acc =
                    match tokens with
                    | [] -> Some(List.rev acc)
                    | tok :: rest ->
                        match tok with
                        | "Cd" -> loop rest (reg crList[min registerBitExt (crList.Length - 1)] :: acc)
                        | "Dd" -> loop rest (reg drList[min registerBitExt (drList.Length - 1)] :: acc)

                        | "Rd" ->
                            if modBits <> 3uy then None
                            else loop rest (reg (registerNameFor "Rd" rmBitExt) :: acc)

                        | "ST0" -> loop rest (reg "ST(0)" :: acc)
                        | "STi" | "Wn" ->
                            if modBits <> 3uy then None
                            else loop rest (reg $"ST(%d{int rmBit})" :: acc)

                        | t when t.StartsWith("E") || t.StartsWith("M") || t.StartsWith("Q") || t.StartsWith("N") ->
                            loop rest (mkRm t :: acc)

                        | t when t.StartsWith("G") ->
                            loop rest (reg (registerNameFor t registerBitExt) :: acc)

                        | t when t.StartsWith("S") && not (t.StartsWith("ST")) ->
                            match tryGetSegmentRegisterName registerBit with
                            | Some srName -> loop rest (reg srName :: acc)
                            | None -> None // <-- seems to be fail with #UD. STOP
                            
                        | _ when isImmediateToken tok ->
                            let n = immediateSize tok opSize addressSize
                            let readStart = idx
                            match tryReadBytes n idx bytes with
                            | None -> None
                            | Some(ni, bs) ->
                                idx <- ni
                                let part: OperandPart =
                                    match tok with
                                    | "Ibs" | "Jbs" -> SignedImm(int64 (sbyte bs[0]), 1)
                                    | "Jb" ->
                                        let d = int (sbyte bs[0])
                                        Rel(ni + d, 1)
                                    | "Jz" ->
                                        let d =
                                            if n = 4 then int (BitConverter.ToInt32(bs, 0))
                                            else int (BitConverter.ToInt16(bs, 0))
                                        Rel(ni + d, n)
                                    | "Ib" -> Imm(uint64 bs[0], 1)
                                    | "Iw" -> Imm(uint64 (BitConverter.ToUInt16(bs, 0)), 2)
                                    | "Id" -> Imm(uint64 (BitConverter.ToUInt32(bs, 0)), 4)
                                    | "Iz" | "Iv" ->
                                        let v =
                                            if n = 8 then BitConverter.ToUInt64(bs, 0)
                                            elif n = 4 then uint64 (BitConverter.ToUInt32(bs, 0))
                                            else uint64 (BitConverter.ToUInt16(bs, 0))
                                        Imm(v, n)
                                    | "Ob" | "Ov" ->
                                        let v =
                                            if n = 8 then BitConverter.ToUInt64(bs, 0)
                                            elif n = 4 then uint64 (BitConverter.ToUInt32(bs, 0))
                                            else uint64 (BitConverter.ToUInt16(bs, 0))
                                        Abs(v, n)
                                    | "Ap" | "Mp" ->
                                        let seg, off =
                                            if n = 10 then BitConverter.ToUInt16(bs, 8), BitConverter.ToUInt64(bs, 0)
                                            elif n = 6 then BitConverter.ToUInt16(bs, 4), uint64 (BitConverter.ToUInt32(bs, 0))
                                            else BitConverter.ToUInt16(bs, 2), uint64 (BitConverter.ToUInt16(bs, 0))
                                        Far(seg, uint64 (off &&& 0xFFFFFFFFUL))
                                    | _ -> Raw tok
                                loop rest
                                    ({ Part = part; BytesOffset = readStart; BytesLength = n } :: acc)

                        | t ->
                            let expanded = expandRegToken t opSize mode
                            loop rest (reg expanded :: acc)

                match loop operands [] with
                | Some parts -> Some(idx, effectiveOperation.mnemonic, parts)
                | None -> None

    let private loadInterruptVectors jsonPath : Map<byte, FlowControlDescriptor> =
        if not (File.Exists jsonPath) then Map.empty
        else
            jsonPath
            |> File.ReadAllText
            |> JsonSerializer.Deserialize<FlowControlDescriptor[]>
            |> Array.choose (fun i ->
                match Byte.TryParse(i.Code, Globalization.NumberStyles.HexNumber, null) with
                | true, b -> Some(b, i)
                | false, _ -> None)
            |> Map.ofArray

    let private loadOpcodeMap jsonPath : Map<string, Operation> =
        jsonPath
        |> File.ReadAllText
        |> JsonSerializer.Deserialize<Instruction array>
        |> Array.map (fun d ->
            let opcode = d.Opcode.Trim()
            let reg = if d.Reg.HasValue then Some d.Reg.Value else None
            { code = opcode
              register = reg
              mnemonic = d.Mnemonic
              operands = d.Operands |> List.map _.Trim() |> List.filter (fun s -> s <> "")
              modRM = d.ModRM
              prefix = d.IsPrefix })
        |> Array.fold (fun acc op -> Map.add op.code op acc) Map.empty

    let create (opcodesPath: string) (interruptsPath: string option) (mode: CpuArchitectureType) : DecoderState =
        if not (File.Exists opcodesPath) then
            failwith "Can't find Intel opcodes map"

        let allOps = loadOpcodeMap opcodesPath

        let prefixes =
            allOps
            |> Map.filter (fun _ op -> op.prefix)
            |> Map.keys
            |> Seq.map (fun hex -> Convert.ToByte(hex, 16))
            |> Set.ofSeq

        let intTable =
            match interruptsPath with
            | Some p -> loadInterruptVectors p
            | None -> Map.empty

        let cpuMode = CpuMode.fromCpuArchitectureType mode
        { opcodesMap = allOps
          prefixSet = prefixes
          interrupts = intTable
          path = ""
          mode = mode
          cpuMode = cpuMode }

    /// Backward-compatible entry point.
    let createLegacy (opcodesPath: string) (interruptsPath: string option) (is32Bit: bool) =
        create opcodesPath interruptsPath (if is32Bit then I386 else I8086)

    /// Scan instruction prefixes and return (endOffset, PrefixState).
    ///
    /// In Long64 mode, 0x40-0x4F are REX prefixes and must be the *last*
    /// prefix byte before the opcode. In 16/32-bit modes they are INC/DEC.
    let private scanPrefixes (state: DecoderState) (bytes: byte[]) (startOffset: int) =
        let mutable i = startOffset
        let mutable ps = PrefixHelpers.emptyPrefixState
        let mutable rex: RexState option = None
        let mutable keepGoing = true

        while keepGoing && i < bytes.Length do
            let b = bytes.[i]

            if CpuMode.use64 state.cpuMode && b >= 0x40uy && b <= 0x4Fuy then
                rex <- PrefixHelpers.rexOfByte b
                i <- i + 1
                keepGoing <- false
            elif PrefixHelpers.isLegacyPrefix b then
                match b with
                | 0x66uy -> ps <- { ps with Opsize = true }
                | 0x67uy -> ps <- { ps with Adsize = true }
                | 0xF0uy -> ps <- { ps with Lock = true }
                | 0xF2uy -> ps <- { ps with Rep = Some Repne }
                | 0xF3uy -> ps <- { ps with Rep = Some Repz }
                | 0x26uy -> ps <- { ps with Segment = Some Es }
                | 0x2Euy -> ps <- { ps with Segment = Some Cs }
                | 0x36uy -> ps <- { ps with Segment = Some Ss }
                | 0x3Euy -> ps <- { ps with Segment = Some Ds }
                | 0x64uy -> ps <- { ps with Segment = Some Fs }
                | 0x65uy -> ps <- { ps with Segment = Some Gs }
                | _ -> ()
                i <- i + 1
            else
                keepGoing <- false

        { ps with Rex = rex }, i

    let private addressSizeOf (state: DecoderState) (prefix: PrefixState) =
        let has67 = prefix.Adsize
        if CpuMode.use64 state.cpuMode then
            if has67 then 32 else 64
        elif CpuMode.use32 state.cpuMode then
            if has67 && CpuMode.has67Prefix state.cpuMode then 16 else 32
        elif CpuMode.has67Prefix state.cpuMode then
            if has67 then 32 else 16
        else 16

    /// Apply the segment prefix to Mem operands (X86: FS:[0x28]).
    let private applySegmentPrefix (seg: SegmentPrefix) (parts: DecodedOperand list) =
        let segName =
            match seg with
            | Es -> "ES" | Cs -> "CS" | Ss -> "SS"
            | Ds -> "DS" | Fs -> "FS" | Gs -> "GS"
        parts
        |> List.map (fun op ->
            match op.Part with
            | Mem s -> { op with Part = Mem $"%s{segName}:%s{s}" }
            | _ -> op)

    let private processOp
        (state: DecoderState)
        (op: Operation)
        (prefix: PrefixState)
        (rex: RexState option)
        (startOperandIdx: int)
        (hex: string)
        (bytes: byte[])
        : DecodedPart option =

        let hasLock = prefix.Lock
        let opSize = opSizeOf state op prefix rex
        let addressSize = addressSizeOf state prefix

        let parseResult, effectiveMnemonic =
            if op.modRM then
                match
                    resolveModRMOperands
                        state.opcodesMap hex op state.cpuMode opSize addressSize
                        bytes startOperandIdx rex prefix.Segment
                with
                | Some(idx, mnem, parts) -> Some(idx, parts), mnem
                | None -> None, ""
            else
                match
                    resolveNoModRMOperands
                        state.cpuMode opSize addressSize bytes startOperandIdx op.operands rex
                with
                | Some(idx, parts) -> Some(idx, parts), op.mnemonic
                | None -> None, ""

        match parseResult with
        | None -> None
        | Some(idx, parts) ->
            // Segment prefix already applied to Mem inside resolveModRMOperands;
            // for no-ModRM forms (moffs/abs) we still need to wrap Abs too.
            let partsWithSeg =
                match prefix.Segment with
                | Some seg when op.modRM = false ->
                    let segName =
                        match seg with
                        | Es -> "ES" | Cs -> "CS" | Ss -> "SS"
                        | Ds -> "DS" | Fs -> "FS" | Gs -> "GS"
                    parts |> List.map (fun opd ->
                        match opd.Part with
                        | Abs(v, n) -> { opd with Part = Mem $"%s{segName}:[%s{formatImmValue v n}]" }
                        | _ -> opd)
                | _ -> parts

            let sizeSuffix =
                if op.operands |> List.exists (fun s -> s = "Yb" || s = "Xb") then "B"
                elif op.operands |> List.exists (fun s -> s = "Yv" || s = "Xv") then
                    if opSize = 8 then "Q"
                    elif opSize = 4 then "D"
                    else "W"
                else ""

            let repPrefix =
                match prefix.Rep with
                | Some Repz -> "REP "
                | Some Repne -> "REPNE "
                | Some Rep -> "REP "
                | None -> ""

            let lockPrefix = if hasLock then "LOCK " else ""
            let rawMnemonic = effectiveMnemonic + sizeSuffix
            let finalMnemonic = lockPrefix + repPrefix + rawMnemonic

            Some
                { Mnemonic = finalMnemonic
                  RawMnemonic = rawMnemonic
                  Operands = partsWithSeg
                  Length = idx }

    /// Decode a single instruction at `bytes[0..]`, returning structured output.
    let touchOperationStructured (state: DecoderState) (bytes: byte[]) : DecodedPart option =
        if bytes.Length = 0 then None
        else
            let prefix, startIdx = scanPrefixes state bytes 0
            let rex = prefix.Rex

            if startIdx >= bytes.Length then None
            else
                let opcodeByte = bytes[startIdx]

                if opcodeByte = 0x0Fuy && CpuArchitectureType.useWordPrefix state.mode && startIdx + 1 < bytes.Length then
                    let hex = $"0F%02X{bytes[startIdx + 1]}"
                    match Map.tryFind hex state.opcodesMap with
                    | Some op -> processOp state op prefix rex (startIdx + 2) hex bytes
                    | None ->
                        // 0F is an escape; it cannot fall back to POP CS in
                        // Protected/Long modes. Only Real16 has a real 0F opcode.
                        if CpuArchitectureType.useWordPrefix state.mode then None
                        else
                            match Map.tryFind "0F" state.opcodesMap with
                            | Some op -> processOp state op prefix rex (startIdx + 1) "0F" bytes
                            | None -> None
                else
                    let hex = $"%02X{opcodeByte}"
                    match Map.tryFind hex state.opcodesMap with
                    | Some op -> processOp state op prefix rex (startIdx + 1) hex bytes
                    | None -> None

    let formatOperandPart (mode: CpuArchitectureType) (instrOffset: int) (part: OperandPart) : string =
        let ipMask = CpuArchitectureType.getInstructionPtrMask mode

        match part with
        | Reg s -> s
        | Imm(v, sz) -> formatImmValue v sz
        | SignedImm(v, sz) ->
            if v < 0L then
                $"-0x%X{if sz = 1 then (-v) &&& 0xFFL else (-v)}"
            else formatImmValue (uint64 v) sz
        | Rel(target, _) ->
            let abs = (int64 instrOffset + int64 target) &&& ipMask
            if CpuArchitectureType.use64 mode then $"0x%016X{int abs}"
            elif CpuArchitectureType.use32 mode then $"0x%08X{int abs}"
            else $"0x%04X{int abs}"
        | Abs(v, sz) -> $"[%s{formatImmValue v sz}]"
        | Mem s -> s
        | Far(seg, off) ->
            if CpuArchitectureType.use64 mode then $"0x%04X{seg}%016X{off}"
            elif CpuArchitectureType.use32 mode then $"0x%04X{seg}%04X{off}" // %08X 
            else $"0x%04X{seg}%04X{off &&& 0xFFFFUL}"
        | Raw s -> s

    let formatDecodedOperand (mode: CpuArchitectureType) (instrOffset: int) (op: DecodedOperand) : string =
        formatOperandPart mode instrOffset op.Part

    let formatDecodedPart (mode: CpuArchitectureType) (instrOffset: int) (part: DecodedPart) : string =
        let ops =
            part.Operands
            |> List.map (formatDecodedOperand mode instrOffset)
            |> List.filter ((<>) "")
            |> String.concat ", "
        if ops = "" then part.Mnemonic.Trim()
        else $"%s{part.Mnemonic} %s{ops}"

    let touchOperation (state: DecoderState) (instrOffset: int) (bytes: byte[]) : (string * int) option =
        match touchOperationStructured state bytes with
        | None -> None
        | Some part -> Some(formatDecodedPart state.mode instrOffset part, part.Length)

    /// size = 1 -> AH (mov ah, imm8 / mov ax, imm16)
    /// size = 2 -> AX (mov ax, imm16 / xor ax, ax)
    /// size = 4 -> EAX (mov eax, imm32 / mov al, imm8 / xor eax, eax)
    /// size = 8 -> RAX (mov rax, imm64 / mov eax, imm32 / xor eax, eax)
    let private tryGetSyscallNumber (bytes: byte[]) (intOffset: int) (size: int) : uint64 option =
        let readU16 idx = uint64 (BitConverter.ToUInt16(bytes, idx))
        let readU32 idx = uint64 (BitConverter.ToUInt32(bytes, idx))
        let readU64 idx = BitConverter.ToUInt64(bytes, idx)

        match size with
        | 1 ->
            // mov ah, imm8 (B4 ib)
            if intOffset >= 2 && bytes[intOffset - 2] = 0xB4uy then
                Some (uint64 bytes[intOffset - 1])
            // mov ax, imm16 (B8 iw) -> старший байт в AH
            elif intOffset >= 3 && bytes[intOffset - 3] = 0xB8uy then
                Some (uint64 bytes[intOffset - 1])
            // xor ax, ax (31 C0 или 33 C0)
            elif intOffset >= 2 && (bytes[intOffset - 2] = 0x31uy || bytes[intOffset - 2] = 0x33uy) && bytes[intOffset - 1] = 0xC0uy then
                Some 0UL
            else None

        | 2 ->
            // mov ax, imm16 (B8 iw)
            if intOffset >= 3 && bytes[intOffset - 3] = 0xB8uy then
                Some (readU16 (intOffset - 2))
            // xor ax, ax (31 C0 / 33 C0)
            elif intOffset >= 2 && (bytes[intOffset - 2] = 0x31uy || bytes[intOffset - 2] = 0x33uy) && bytes[intOffset - 1] = 0xC0uy then
                Some 0UL
            else None

        | 4 ->
            // mov eax, imm32 (B8 id)
            if intOffset >= 5 && bytes[intOffset - 5] = 0xB8uy then
                Some (readU32 (intOffset - 4))
            // mov al, imm8 (B0 ib) — часто в shellcode/asm после xor eax, eax
            elif intOffset >= 2 && bytes[intOffset - 2] = 0xB0uy then
                Some (uint64 bytes[intOffset - 1])
            // xor eax, eax (31 C0 / 33 C0) -> sys_restart_syscall (0)
            elif intOffset >= 2 && (bytes[intOffset - 2] = 0x31uy || bytes[intOffset - 2] = 0x33uy) && bytes[intOffset - 1] = 0xC0uy then
                Some 0UL
            else None

        | 8 ->
            // mov rax, imm64 (48 B8 iq)
            if intOffset >= 10 && bytes[intOffset - 10] = 0x48uy && bytes[intOffset - 9] = 0xB8uy then
                Some (readU64 (intOffset - 8))
            // mov eax, imm32 (B8 id — в x86-64 обнуляет верхние 32 бита RAX)
            elif intOffset >= 5 && bytes[intOffset - 5] = 0xB8uy then
                Some (readU32 (intOffset - 4))
            // xor eax, eax (31 C0 / 33 C0)
            elif intOffset >= 2 && (bytes[intOffset - 2] = 0x31uy || bytes[intOffset - 2] = 0x33uy) && bytes[intOffset - 1] = 0xC0uy then
                Some 0UL
            else None

        | _ -> None
    
    let private tryResolveAccumulatorFromHistory
        (blockHistory: DecodedInstruction list)
        (size: int)
        : uint64 option =

        let knownBytes : byte option[] = Array.create 8 None

        let writeImm (startIdx: int) (count: int) (value: uint64) =
            for i in 0 .. count - 1 do
                if startIdx + i < 8 && knownBytes[startIdx + i].IsNone then
                    let b = byte ((value >>> (i * 8)) &&& 0xFFUL)
                    knownBytes[startIdx + i] <- Some b

        let tryAssemble () : uint64 option =
            match size with
            | 1 ->
                knownBytes[1] |> Option.map uint64

            | 2 | 4 | 8 ->
                let mutable allKnown = true
                let mutable acc = 0UL
                for i in 0 .. size - 1 do
                    match knownBytes[i] with

                    | Some b -> acc <- acc ||| (uint64 b <<< (i * 8))
                    | None -> allKnown <- false
                if allKnown then Some acc else None
            | _ -> None

        let regSpan (regName: string) : (int * int) option =
            match regName.ToUpperInvariant() with
            | "AL" -> Some(0, 1)
            | "AH" -> Some(1, 1)
            | "AX" -> Some(0, 2)
            | "EAX" -> Some(0, 4)
            | "RAX" -> Some(0, 8)
            | _ -> None

        let tryGetImm (op: DecodedOperand) : uint64 option =
            match op.Part with
            | Imm(v, _) -> Some v
            | SignedImm(v, _) -> Some (uint64 v)
            | _ -> None

        let rec walk (history: DecodedInstruction list) : uint64 option =
            match tryAssemble () with
            | Some res -> Some res
            | None ->
                match history with
                | [] -> None
                | instr :: rest ->
                    // call might clob EAX/AX/AH
                    if instr.Flow = Call || instr.Flow = IndirectCall || instr.Flow = IndirectCallFar then
                        None
                    else
                        let mnem = instr.RawMnemonic.ToUpperInvariant()
                        let ops = instr.Operands

                        match mnem, ops with
                        // MOV reg, imm

                        | "MOV", [| dst; src |] ->
                            match dst.Part with
                            | Reg rName ->
                                match regSpan rName, tryGetImm src with
                                | Some(startIdx, count), Some immVal ->
                                    writeImm startIdx count immVal
                                    if rName = "EAX" then
                                        writeImm 4 4 0UL
                                    walk rest
                                | Some(startIdx, count), None ->
                                    let targetStart, targetEnd = if size = 1 then 1, 1 else 0, size - 1
                                    let writeEnd = startIdx + count - 1
                                    let intersects = not (writeEnd < targetStart || startIdx > targetEnd)
                                    if intersects then None else walk rest
                                | None, _ ->
                                    walk rest
                            | _ -> walk rest

                        // XOR reg, reg / SUB reg, reg -> reg became clobber 

                        | ("XOR" | "SUB"), [| op1; op2 |] ->
                            match op1.Part, op2.Part with
                            | Reg r1, Reg r2 when r1 = r2 ->
                                match regSpan r1 with
                                | Some(startIdx, count) ->
                                    writeImm startIdx count 0UL
                                    if r1 = "EAX" then writeImm 4 4 0UL
                                    walk rest
                                | None -> walk rest
                            | Reg r1, _ ->
                                match regSpan r1 with
                                | Some _ -> None // unknown acc.
                                | None -> walk rest
                            | _ -> walk rest

                        | _, ops when ops.Length >= 1 ->
                            match ops[0].Part with
                            | Reg rName ->
                                match regSpan rName with
                                | Some(startIdx, count) ->
                                    let targetStart, targetEnd = if size = 1 then 1, 1 else 0, size - 1
                                    let writeEnd = startIdx + count - 1
                                    let intersects = not (writeEnd < targetStart || startIdx > targetEnd)
                                    // CMP/TEST doesn't clob
                                    if intersects && mnem <> "CMP" && mnem <> "TEST" then None
                                    else walk rest
                                | None -> walk rest
                            | _ -> walk rest

                        | _ -> walk rest

        walk blockHistory
    
    let private tryParseHexUInt64 (hex: string) : uint64 option =
        match UInt64.TryParse(hex.Trim(), Globalization.NumberStyles.HexNumber, null) with
        | true, v -> Some v
        | false, _ -> None

    let private resolveInterruptFlow
        (state: DecoderState)
        (opcodeOffset: int)
        (bytes: byte[])
        (blockHistory: DecodedInstruction list)
        : FlowControl * int list * string option =

        if opcodeOffset + 1 >= bytes.Length then
            SoftwareInterrupt, [], None
        else
            let intNum = bytes[opcodeOffset + 1]
            match Map.tryFind intNum state.interrupts with
            | None -> SoftwareInterrupt, [], None
            | Some intDef ->
                let regCache = System.Collections.Generic.Dictionary<int, uint64 option>()
                let getRegVal sz =
                    match regCache.TryGetValue sz with
                    | true, v -> v
                    | false, _ ->
                        let v = tryResolveAccumulatorFromHistory blockHistory sz
                        regCache[sz] <- v
                        v

                let matchedArg =
                    intDef.Args
                    |> Array.tryFind (fun a ->
                        if a.ArgByte = "*" then true
                        else
                            match getRegVal a.Size, tryParseHexUInt64 a.ArgByte with
                            | Some regVal, Some targetVal -> regVal = targetVal
                            | _ -> false)

                match matchedArg with
                | Some arg ->
                    let flow = if arg.ReturnFlow then FallThrough else Terminate
                    flow, [], Some arg.Comment
                | None -> SoftwareInterrupt, [], None
    
    let private classifyFlow
        (state: DecoderState) (offset: int) (length: int) (bytes: byte[]) (blockHistory: DecodedInstruction list)
        : FlowControl * int list * string option =

        let ipMask = CpuArchitectureType.getInstructionPtrMask state.mode
        let _, opcodeOffset = scanPrefixes state bytes offset

        if opcodeOffset >= bytes.Length then FallThrough, [], None
        else
            let opcode = bytes[opcodeOffset]

            let targetOf (rel: int64) =
                [ int ((int64 offset + int64 length + rel) &&& ipMask) ]

            let readRel8 () =
                let r = opcodeOffset + 1
                if r < bytes.Length then Some(int64 (sbyte bytes[r])) else None

            let readRel16 () =
                let r = opcodeOffset + 1
                if r + 2 <= bytes.Length then Some(int64 (BitConverter.ToInt16(bytes, r))) else None

            let readRel32 () =
                let r = opcodeOffset + 1
                if r + 4 <= bytes.Length then Some(int64 (BitConverter.ToInt32(bytes, r))) else None

            // rel32 is unconditional in long mode; in 16/32-bit it depends
            // on the effective operand size.
            let readRelAuto () =
                if CpuMode.use64 state.cpuMode then readRel32 ()
                else
                    let prefix, _ = scanPrefixes state bytes offset
                    let rex = prefix.Rex
                    let has66 = prefix.Opsize && CpuMode.has66Prefix state.cpuMode
                    let rexW = match rex with Some r -> r.W | None -> false
                    let opSize = operandSize state.cpuMode rexW has66
                    if opSize = 4 then readRel32 () else readRel16 ()

            if opcode = 0x0Fuy && CpuArchitectureType.useWordPrefix state.mode then
                let b2Offset = opcodeOffset + 1
                if b2Offset >= bytes.Length then FallThrough, [], None
                else
                    let b2 = bytes[b2Offset]
                    if b2 >= 0x80uy && b2 <= 0x8Fuy then
                        match readRelAuto () with
                        | Some r -> ConditionalJump, targetOf r, None
                        | None -> FallThrough, [], None
                    elif b2 = 0x05uy then SoftwareInterrupt, [], Some "syscall"
                    elif b2 = 0x07uy then Return, [], None
                    else FallThrough, [], None
            else
                match opcode with
                | 0xEBuy -> (match readRel8 () with Some r -> Jump, targetOf r, None | None -> FallThrough, [], None)
                | 0xE9uy -> (match readRelAuto () with Some r -> Jump, targetOf r, None | None -> FallThrough, [], None)
                | 0xE8uy -> (match readRelAuto () with Some r -> Call, targetOf r, None | None -> FallThrough, [], None)
                | 0x70uy | 0x71uy | 0x72uy | 0x73uy | 0x74uy | 0x75uy | 0x76uy | 0x77uy
                | 0x78uy | 0x79uy | 0x7Auy | 0x7Buy | 0x7Cuy | 0x7Duy | 0x7Euy | 0x7Fuy ->
                    (match readRel8 () with Some r -> ConditionalJump, targetOf r, None | None -> FallThrough, [], None)
                | 0xE0uy | 0xE1uy | 0xE2uy | 0xE3uy ->
                    (match readRel8 () with Some r -> ConditionalJump, targetOf r, None | None -> FallThrough, [], None)
                | 0xCDuy -> resolveInterruptFlow state opcodeOffset bytes blockHistory
                | 0xCEuy -> FallThrough, [], None
                | 0xC2uy | 0xC3uy | 0xCBuy | 0xCFuy -> Return, [], None
                | 0xF4uy -> Halt, [], None
                | 0xFFuy ->
                    if opcodeOffset + 1 < bytes.Length then
                        let modrm = bytes[opcodeOffset + 1]
                        let reg = (modrm >>> 3) &&& 0b111uy
                        match reg with
                        | 2uy -> IndirectCall, [], None
                        | 3uy -> IndirectCallFar, [], None
                        | 4uy -> IndirectJump, [], None
                        | 5uy -> IndirectJumpFar, [], None
                        | _ -> FallThrough, [], None
                    else FallThrough, [], None
                | 0xEAuy -> Jump, [], None
                | 0x9Auy -> Call, [], None
                | _ -> FallThrough, [], None

    let decodeInstructionWithHistory
        (state: DecoderState)
        (bytes: byte[])
        (offset: int)
        (blockHistory: DecodedInstruction list)
        : DecodedInstruction option =
        if offset < 0 || offset >= bytes.Length then None
        else
            let slice = bytes[offset..]
            match touchOperationStructured state slice with
            | None -> None
            | Some part ->
                let length = part.Length
                if length <= 0 || offset + length > bytes.Length then None
                else
                    let flow, targets, comment = classifyFlow state offset length bytes blockHistory
                    let instrBytes = bytes[offset .. offset + length - 1]
                    Some
                        { Offset = offset
                          Length = length
                          Mnemonic = part.Mnemonic
                          RawMnemonic = part.RawMnemonic
                          Operands = part.Operands |> List.toArray
                          Bytes = instrBytes
                          Flow = flow
                          Targets = targets
                          Comment = comment }

    let decodeInstruction (state: DecoderState) (bytes: byte[]) (offset: int) : DecodedInstruction option =
        decodeInstructionWithHistory state bytes offset []

    let runLinear (state: DecoderState) (bytes: byte[]) : DisassembledInstruction list =
        let rec loop offset acc =
            if offset >= bytes.Length then List.rev acc
            else
                let slice = bytes[offset..]
                match touchOperation state offset slice with
                | Some(mnemonic, length) when length > 0 && offset + length <= bytes.Length ->
                    let instrBytes = bytes[offset .. offset + length - 1]
                    loop (offset + length)
                        ({ Offset = offset; Length = length; Mnemonic = mnemonic; Bytes = instrBytes } :: acc)
                | _ ->
                    loop (offset + 1)
                        ({ Offset = offset; Length = 1; Mnemonic = "???"; Bytes = [| bytes[offset] |] } :: acc)
        loop 0 []

    let format (instructions: DisassembledInstruction list) =
        instructions
        |> List.map (fun instr ->
            let addr = $"0x%04X{instr.Offset}"
            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " 0x")
            $"%s{addr}  [0x%s{bytesStr}]  %s{instr.Mnemonic}")

    let runRecursive
        (state: DecoderState) (bytes: byte[]) (entryPoints: int[])
        : DecodedInstruction list * ByteStatus[] * Set<int> =

        let status = Array.create bytes.Length ByteStatus.Unknown
        let mutable result : DecodedInstruction list = []
        let queue = System.Collections.Generic.Queue<int>()
        let entrySet = Set.ofArray entryPoints

        let rec decodeChain (offset: int) (history: DecodedInstruction list) =
            if offset < 0 || offset >= bytes.Length then ()
            elif status[offset] <> ByteStatus.Unknown then ()
            else
                match decodeInstructionWithHistory state bytes offset history with
                | None -> status[offset] <- ByteStatus.Data
                | Some instr ->
                    let overlaps =
                        let mutable hit = false
                        let mutable i = 1
                        while not hit && i < instr.Length && offset + i < bytes.Length do
                            if status[offset + i] = ByteStatus.Code then hit <- true
                            i <- i + 1
                        hit
                    if not overlaps then
                        for i in 0 .. instr.Length - 1 do
                            if offset + i < bytes.Length then status[offset + i] <- ByteStatus.Code
                        result <- instr :: result
                        for t in instr.Targets do
                            if t >= 0 && t < bytes.Length && status[t] = ByteStatus.Unknown then queue.Enqueue t
                        let fallThrough = offset + instr.Length
                        let hasFall =
                            match instr.Flow with

                            | FallThrough | ConditionalJump | Call
                            | IndirectCall | IndirectCallFar -> true
                            | _ -> false
                        if hasFall then
                            // If Fallthrough is a jump target, I need to reset all history.
                            // For now it's better to store instr :: history tail 
                            decodeChain fallThrough (instr :: history)

        for ep in entryPoints do decodeChain ep []
        while queue.Count > 0 do decodeChain (queue.Dequeue()) []

        (List.rev result, status, entrySet)

    let private zeroRunThreshold = 4
    let private dataBytesPerLine = 16
    let private maxDataLinesPerGap = 16
    ///
    /// Still under construction
    /// Without stack analysis and correct image/object mapping this function emits
    /// details program text as data. Discarded by default for a some time   
    /// 
    let private emitDataRange (sb: StringBuilder) (bytes: byte[]) (offset: int) (length: int) =
        let mutable linesLeft = maxDataLinesPerGap

        let emitRaw (start: int) (count: int) =
            let mutable left = count
            let mutable pos = start
            while left > 0 && linesLeft > 0 do
                let take = min dataBytesPerLine left
                let payload =
                    bytes[pos .. pos + take - 1]
                    |> Array.map (fun b -> $"0x%02X{b}")
                    |> String.concat ", "
                sb.AppendLine(sprintf "\t%-30s ; 0x%04X data" ("DB " + payload) pos) |> ignore
                pos <- pos + take
                left <- left - take
                linesLeft <- linesLeft - 1
            if left > 0 then
                sb.AppendLine $"; 0x%04X{pos} %d{left} more byte(s) of data (omitted)" |> ignore

        let limit = offset + length
        let mutable i = offset
        while i < limit do
            if bytes[i] = 0uy then
                let start = i
                while i < limit && bytes[i] = 0uy do i <- i + 1
                let count = i - start
                if count >= zeroRunThreshold then
                    let duplicates = $"DB %d{count} DUP 0x00"
                    sb.AppendLine $"\t%-30s{duplicates} ; 0x%04X{start} alignment padding" |> ignore
                else emitRaw start count
            else
                let start = i
                while i < limit && bytes[i] <> 0uy do i <- i + 1
                emitRaw start (i - start)

    let formatWithLabels
        (instructions: DecodedInstruction list) (bytes: byte[])
        (status: ByteStatus[]) (entrySet: Set<int>) =

        let labelSet = instructions |> List.collect _.Targets |> Set.ofList
        let callTargets =
            instructions |> List.filter (fun i -> i.Flow = Call)
            |> List.collect _.Targets |> Set.ofList

        let sortedInstructions = instructions |> List.sortBy _.Offset

        let terminalText (flow: FlowControl) =
            match flow with
            | Return -> Some "returned to previous control flow"
            | Terminate -> Some "program terminates here"
            | Halt -> Some "CPU halted"
            | _ -> None

        let mutable prevWasTerm = false
        let mutable coverage = 0
        let sb = StringBuilder()

        for instr in sortedInstructions do
            // if instr.Offset > coverage then
            //     emitDataRange sb bytes coverage (instr.Offset - coverage)
            coverage <- max coverage (instr.Offset + instr.Length)

            let isEntry = entrySet.Contains instr.Offset
            let isCall = callTargets.Contains instr.Offset
            let isLabel = labelSet.Contains instr.Offset && not isCall && not isEntry

            if isEntry || isCall then
                let header =
                    match isEntry, isCall with
                    | true, true -> Some $"; Entry point & procedure at 0x%04X{instr.Offset}"
                    | true, false -> Some $"; Entry point at 0x%04X{instr.Offset} (export)"
                    | false, true -> Some $"; Procedure at 0x%04X{instr.Offset} instruction offset"
                    | _ -> None
                match header with
                | Some h ->
                    if not prevWasTerm then sb.AppendLine ";" |> ignore
                    sb.AppendLine h |> ignore
                    sb.AppendLine ";" |> ignore
                | None -> ()
                sb.AppendLine $"p_0x%04X{instr.Offset}:" |> ignore
                prevWasTerm <- false
            elif isLabel then
                sb.AppendLine $"__0x%04X{instr.Offset}:" |> ignore

            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " ")
            let comment = match instr.Comment with Some c -> c | None -> ""
            sb.AppendLine $"\t%-30s{instr.Mnemonic} ; 0x%04X{instr.Offset} %s{bytesStr} %s{comment}"
            |> ignore

            match terminalText instr.Flow with
            | Some text ->
                sb.AppendLine $"; %s{text}" |> ignore
                sb.AppendLine() |> ignore
                prevWasTerm <- true
            | None -> prevWasTerm <- false

        // if coverage < bytes.Length then
        //     emitDataRange sb bytes coverage (bytes.Length - coverage)

        let decodedOffsets = sortedInstructions |> List.map _.Offset |> Set.ofList
        let missedEntries = Set.difference entrySet decodedOffsets

        if not missedEntries.IsEmpty then
            sb.AppendLine() |> ignore
            sb.AppendLine "; WARNING: the following entry points do not align with any instruction boundary:"
            |> ignore
            for ep in missedEntries do
                sb.AppendLine $" ;   0x%04X{ep}" |> ignore

        sb.ToString()
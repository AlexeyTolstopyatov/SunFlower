//
// CoffeeLake (C) 2026
// This module uses various external sources.
// In example: "./opcodes8086.json". If this database file is missing,
// next following instructions are be aborted. (raises failure)
//
// Template header of disassembled code declared in the "./head.txt"
//
// This module represents common logic for I8086 and I186 disassembler
// Connecting opcodes map of I286+ will return incorrect bytes reinterpretation!
//
namespace Sunflower.Dasm.Intel.Core

open System
open System.IO
open System.Text
open System.Text.Json

type DecoderState =
    { opcodesMap: Map<string, Operation>
      prefixSet: Set<byte>
      interrupts: Map<byte, IntelInterrupt>
      path: string
      is32Bit: bool }

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
    /// Used for unknown software interrupts or [INTO]
    | SoftwareInterrupt

type DecodedInstruction =
    { Offset: int
      Length: int
      Mnemonic: string
      Bytes: byte array
      Flow: FlowControl
      Targets: int list
      Comment: string option }

type ByteStatus =
    | Unknown = 0
    | Code = 1
    | Data = 2


module internal Decoder =
    let private loadInterruptVectors jsonPath : Map<byte, IntelInterrupt> =
        if not (File.Exists jsonPath) then
            Map.empty
        else
            jsonPath
            |> File.ReadAllText
            |> JsonSerializer.Deserialize<IntelInterrupt[]>
            |> Array.choose (fun i ->
                match Byte.TryParse(i.Code, System.Globalization.NumberStyles.HexNumber, null) with
                | true, b -> Some(b, i)
                | false, _ -> None)
            |> Map.ofArray

    let private loadOpcodeMap jsonPath =
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
        |> Array.map (fun op ->
            match Byte.TryParse(op.code, System.Globalization.NumberStyles.HexNumber, null) with
            | true, hexCode ->
                let noModRM =
                    match hexCode with
                    | 0x50uy
                    | 0x51uy
                    | 0x52uy
                    | 0x53uy
                    | 0x54uy
                    | 0x55uy
                    | 0x56uy
                    | 0x57uy
                    | 0x58uy
                    | 0x59uy
                    | 0x5Auy
                    | 0x5Buy
                    | 0x5Cuy
                    | 0x5Duy
                    | 0x5Euy
                    | 0x5Fuy
                    | 0x90uy
                    | 0x91uy
                    | 0x92uy
                    | 0x93uy
                    | 0x94uy
                    | 0x95uy
                    | 0x96uy
                    | 0x97uy
                    | 0xB8uy
                    | 0xB9uy
                    | 0xBAuy
                    | 0xBBuy
                    | 0xBCuy
                    | 0xBDuy
                    | 0xBEuy
                    | 0xBFuy -> true
                    | _ -> false

                if noModRM then { op with modRM = false } else op
            | false, _ -> op)
        |> Array.fold (fun acc op -> Map.add op.code op acc) Map.empty

    let create (opcodesPath: string) (interruptsPath: string option) (is32Bit: bool) =
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

        { opcodesMap = allOps
          prefixSet = prefixes
          interrupts = intTable
          path = ""
          is32Bit = is32Bit }


    let private reg8Names = [| "AL"; "CL"; "DL"; "BL"; "AH"; "CH"; "DH"; "BH" |]
    let private reg16Names = [| "AX"; "CX"; "DX"; "BX"; "SP"; "BP"; "SI"; "DI" |]
    let private reg32Names = [| "EAX"; "ECX"; "EDX"; "EBX"; "ESP"; "EBP"; "ESI"; "EDI" |]
    let private segNames = [| "ES"; "CS"; "SS"; "DS"; "FS"; "GS" |]

    /// <summary>
    /// 16-bit effective address calculation.
    /// </summary>
    let private getEffectiveAddress16 rm =
        match rm with
        | 0uy -> "[BX+SI]"
        | 1uy -> "[BX+DI]"
        | 2uy -> "[BP+SI]"
        | 3uy -> "[BP+DI]"
        | 4uy -> "[SI]"
        | 5uy -> "[DI]"
        | 6uy -> "[BP]"
        | 7uy -> "[BX]"
        | _ -> failwith "invalid rm"

    /// <summary>
    /// 32-bit effective address calculation supporting SIB byte.
    /// Returns the address string and updated index past the SIB+disp.
    /// </summary>
    let private getEffectiveAddress32 rm (modBits: byte) (idx: int) (bytes: byte[]) =
        let reg32NamesEA = [| "EAX"; "ECX"; "EDX"; "EBX"; "ESP"; "EBP"; "ESI"; "EDI" |]

        // --- SIB decoding ---
        let sibScaled (scale: byte) (indexReg: byte) =
            let scaleFactor = 1 <<< int scale
            let indexName = reg32NamesEA[int indexReg]
            if indexReg = 4uy then "" // ESP cannot be an index
            elif scaleFactor = 1 then indexName
            else $"{indexName}*{scaleFactor}"

        if rm = 4uy then
            // SIB byte present
            if idx >= bytes.Length then
                None // not enough data
            else
                let sib = bytes[idx]
                let scale = (sib >>> 6) &&& 0b11uy
                let indexReg = (sib >>> 3) &&& 0b111uy
                let baseReg = sib &&& 0b111uy
                let mutable afterSib = idx + 1

                let baseName =
                    match baseReg with
                    | 5uy when modBits = 0uy -> None  // [*+disp32] – no base, just disp
                    | _ -> Some(reg32NamesEA[int baseReg])

                let indexPart = sibScaled scale indexReg

                // Read displacement
                let disp, newIdx =
                    match modBits with
                    | 1uy ->
                        if afterSib < bytes.Length then
                            Some(int8 bytes[afterSib] |> int), afterSib + 1
                        else None, afterSib
                    | 2uy ->
                        if afterSib + 3 < bytes.Length then
                            Some(BitConverter.ToInt32(bytes, afterSib)), afterSib + 4
                        else None, afterSib
                    | 0uy when baseReg = 5uy ->
                        if afterSib + 3 < bytes.Length then
                            Some(BitConverter.ToInt32(bytes, afterSib)), afterSib + 4
                        else None, afterSib
                    | _ -> None, afterSib

                afterSib <- newIdx

                let parts =
                    [ match baseName with
                      | Some bn -> bn
                      | None -> ()
                      if indexPart <> "" then
                          if baseName.IsSome then "+"
                          indexPart
                      match disp with
                      | Some d when d > 0 -> $"+0x%X{d}"
                      | Some d when d < 0 -> $"-0x%X{-d}"
                      | _ -> () ]
                    |> List.reduce (+)

                let eaStr = if parts = "" then "[0x0]" else $"[{parts}]"
                Some(eaStr, afterSib)

        else
            // No SIB, direct register or [base+disp]
            let baseAddress =
                if modBits = 3uy then
                    reg32NamesEA[int rm]  // register form
                else
                    let baseReg = reg32NamesEA[int rm]
                    $"[{baseReg}]"

            let mutable afterRm = idx

            let dispStr, finalIdx =
                match modBits with
                | 1uy ->
                    if afterRm < bytes.Length then
                        let d = int8 bytes[afterRm] |> int
                        afterRm <- afterRm + 1
                        let disp = if d >= 0 then $"+0x%X{d}" else $"-0x%X{-d}"

                        let result =
                            if rm = 6uy && modBits = 0uy then
                                $"[0x%08X{uint32 (byte d)}]"
                            else
                                let core = reg32NamesEA[int rm]
                                $"[{core}{disp}]"

                        result, afterRm
                    else baseAddress, afterRm
                | 2uy ->
                    if afterRm + 3 < bytes.Length then
                        let d = BitConverter.ToInt32(bytes, afterRm)
                        afterRm <- afterRm + 4
                        let disp = if d >= 0 then $"+0x%X{d}" else $"-0x%X{-d}"
                        let result = reg32NamesEA[int rm] |> fun r -> $"[{r}{disp}]"
                        result, afterRm
                    else baseAddress, afterRm
                | 0uy when rm = 5uy ->
                    // [disp32] – direct address
                    if afterRm + 3 < bytes.Length then
                        let disp = BitConverter.ToUInt32(bytes, afterRm)
                        afterRm <- afterRm + 4
                        $"[0x%08X{disp}]", afterRm
                    else baseAddress, afterRm
                | _ -> baseAddress, afterRm

            Some(dispStr, finalIdx)

    let private formatImmediate (bytes: byte[]) =
        if bytes = null || bytes.Length = 0 then
            "0x0"
        else
            match bytes.Length with
            | 1 -> $"0x%02X{bytes[0]}"
            | 2 -> $"0x%04X{BitConverter.ToUInt16(bytes, 0)}"
            | 4 -> $"0x%08X{BitConverter.ToUInt32(bytes, 0)}"
            | _ -> "0x" + BitConverter.ToString(bytes).Replace("-", "")

    let private immediateSize (token: string) (hasOpSize32: bool) (addressSize: int) =
        match token with
        | "Ib"
        | "Jb" -> 1
        | "Iw" -> 2
        | "Id" -> 4
        | "Iz"
        | "Iv"
        | "Jz" -> if hasOpSize32 then 4 else 2
        | "Ob"
        | "Ov" -> if addressSize = 32 then 4 else 2
        | "Mp"
        | "Ap"
        | "p" -> 4
        | t -> failwithf $"Unsupported immediate token: %s{t}"

    let private tryReadByte (i: int) (bytes: byte[]) =
        if i < bytes.Length then Some(i + 1, bytes[i]) else None

    let private tryReadBytes (n: int) (i: int) (bytes: byte[]) =
        if i + n <= bytes.Length then
            Some(i + n, bytes[i .. i + n - 1])
        else
            None

    let private resolveNoModRMOperands bytes startIdx operands hasOpSize32 addressSize is32Bit =
        let rec loop idx tokens acc =
            match tokens with
            | [] -> Some(idx, List.rev acc)
            | token :: rest ->
                match token with
                | "AL"
                | "CL"
                | "DL"
                | "BL"
                | "AH"
                | "CH"
                | "DH"
                | "BH" -> loop idx rest (token :: acc)
                | "rAX"
                | "eAX" -> loop idx rest ((if is32Bit && hasOpSize32 then "EAX" else "AX") :: acc)
                | "rCX"
                | "eCX" -> loop idx rest ((if is32Bit && hasOpSize32 then "ECX" else "CX") :: acc)
                | "rDX"
                | "eDX" -> loop idx rest ((if is32Bit && hasOpSize32 then "EDX" else "DX") :: acc)
                | "rBX"
                | "eBX" -> loop idx rest ((if is32Bit && hasOpSize32 then "EBX" else "BX") :: acc)
                | "rSP"
                | "eSP" -> loop idx rest ((if is32Bit && hasOpSize32 then "ESP" else "SP") :: acc)
                | "rBP"
                | "eBP" -> loop idx rest ((if is32Bit && hasOpSize32 then "EBP" else "BP") :: acc)
                | "rSI"
                | "eSI" -> loop idx rest ((if is32Bit && hasOpSize32 then "ESI" else "SI") :: acc)
                | "rDI"
                | "eDI" -> loop idx rest ((if is32Bit && hasOpSize32 then "EDI" else "DI") :: acc)
                | "ES"
                | "CS"
                | "SS"
                | "DS"
                | "FS"
                | "GS" -> loop idx rest (token :: acc)
                | "Ib" ->
                    match tryReadByte idx bytes with
                    | None -> None
                    | Some(ni, b) -> loop ni rest ($"0x%02X{b}" :: acc)
                | "Iz"
                | "Iv" ->
                    let n = if hasOpSize32 then 4 else 2

                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) -> loop ni rest (formatImmediate bs :: acc)
                | "Jb" ->
                    match tryReadByte idx bytes with
                    | None -> None
                    | Some(ni, rel) ->
                        let offset = int8 rel |> int
                        let mask = if is32Bit && hasOpSize32 then 0xFFFFFFFF else 0xFFFF
                        let targetAddr = (ni + offset) &&& mask
                        loop ni rest ($"0x%04X{targetAddr}" :: acc)
                | "Jz" ->
                    let n = if hasOpSize32 then 4 else 2

                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) ->
                        let rel =
                            if n = 2 then
                                BitConverter.ToInt16(bs, 0) |> int
                            else
                                BitConverter.ToInt32(bs, 0)

                        let mask = if is32Bit && hasOpSize32 then 0xFFFFFFFF else 0xFFFF
                        let targetAddr = (ni + rel) &&& mask
                        loop ni rest ($"0x%04X{targetAddr}" :: acc)
                | "Ob"
                | "Ov" ->
                    let n = if addressSize = 32 then 4 else 2

                    match tryReadBytes n idx bytes with
                    | None -> None
                    | Some(ni, bs) -> loop ni rest ($"[%s{formatImmediate bs}]" :: acc)
                | "Xb"
                | "Yb"
                | "Xv"
                | "Yv"
                // | "Ap" ->  

                | "Fv" -> loop idx rest ("" :: acc)
                | t when t.Contains('/') ->
                    let simple = t.Split('/').[0].Trim()
                    loop idx (simple :: rest) acc
                | other -> loop idx rest (other :: acc)

        loop startIdx operands []

    /// <summary>
    /// Returns true if the token is an immediate or jump-displacement token
    /// that should be read from the byte stream rather than used as a register.
    /// </summary>
    let private isImmediateToken (token: string) =
        match token with
        | "Ib"
        | "Iv"
        | "Iz"
        | "Id"
        | "Iw"
        | "Jb"
        | "Jz"
        | "Ob"
        | "Ov"
        | "Mp"
        | "Ap" -> true
        | _ -> false

    let rec private resolveModRMOperands opcodesMap hex defaultOperation bytes startIdx hasOpSize32 addressSize is32Bit =
        match tryReadByte startIdx bytes with
        | None -> None
        | Some(idx, modrm) ->
            let modBits = (modrm >>> 6) &&& 0b11uy
            let reg = (modrm >>> 3) &&& 0b111uy
            let rm = modrm &&& 0b111uy

            let effectiveOp =
                let groupKey = $"{hex}/{reg}"

                match Map.tryFind groupKey opcodesMap with
                | Some grpOp -> grpOp
                | None -> defaultOperation

            let operands = effectiveOp.operands

            if operands.Length = 0 then
                // Base group entry with no operands (e.g. opcode "80" alone) —
                // fall back to the group variant, but if it also has no operands, fail.
                let groupFallbackKey = $"{hex}/{reg}"

                match Map.tryFind groupFallbackKey opcodesMap with
                | Some fallbackOp when fallbackOp.operands.Length > 0 ->
                    resolveModRMOperands opcodesMap hex fallbackOp bytes startIdx hasOpSize32 addressSize is32Bit
                | _ -> None
            else
                let firstToken = operands[0]
                let isByte = firstToken.EndsWith("b")

                // Choose register name table based on operand size
                let regNames =
                    if isByte then reg8Names
                    elif is32Bit && hasOpSize32 then reg32Names
                    else reg16Names

                let regName (r: byte) =
                    let idx = int r
                    regNames[idx]

                let hasSegReg = effectiveOp.operands |> List.exists (fun t -> t.Contains("S"))
                let regStr = if hasSegReg then segNames[int reg] else regName reg
                let mutable idxAfterRm = idx

                let rmStrOpt =
                    if modBits = 3uy then
                        // Register operand (not memory)
                        Some(regName rm, idxAfterRm)
                    else
                        // Memory operand
                        let baseAddress, hasSib =
                            if addressSize = 32 then
                                // 32-bit addressing: SIB possible
                                (None, rm = 4uy)
                            else
                                (Some(getEffectiveAddress16 rm), false)

                        if addressSize = 32 then
                            getEffectiveAddress32 rm modBits idxAfterRm bytes
                            |> Option.map (fun (eaStr, newIdx) ->
                                idxAfterRm <- newIdx
                                (eaStr, idxAfterRm))
                        else
                            let dispBytes =
                                match modBits with
                                | 1uy -> tryReadByte idxAfterRm bytes |> Option.map (fun (ni, b) -> ni, [| b |])
                                | 2uy -> tryReadBytes (if addressSize = 32 then 4 else 2) idxAfterRm bytes
                                | _ -> Some(idxAfterRm, Array.empty)

                            match dispBytes with
                            | None -> None
                            | Some(newIdx, disp) ->
                                idxAfterRm <- newIdx

                                let dispStr =
                                    if disp.Length > 0 then
                                        let extDisp =
                                            if disp.Length = 1 then
                                                uint32 (int8 disp[0])
                                            else
                                                uint32 (BitConverter.ToInt16(disp, 0))
                                        (if (sbyte disp[0]) < 0y then "-" else "+") + formatImmediate (if disp.Length = 1 then [| disp[0] |] else disp)
                                    else
                                        ""

                                let rmString = match baseAddress with Some ba -> ba + dispStr | None -> "[??]"
                                Some(rmString, idxAfterRm)

                match rmStrOpt with
                | None -> None
                | Some(rmStr, newIdx) ->
                    let rec loopTokens
                        (idx: int)
                        (tokens: string list)
                        (acc: string list)
                        : (int * string list) option =
                        match tokens with
                        | [] -> Some(idx, List.rev acc)
                        | tok :: rest ->
                            if tok.Contains("E") then
                                loopTokens idx rest (rmStr :: acc)
                            elif tok.Contains("G") then
                                loopTokens idx rest (regStr :: acc)
                            elif tok.Contains("S") then
                                loopTokens idx rest (regStr :: acc)
                            elif tok.StartsWith("M") then
                                loopTokens idx rest (rmStr :: acc)
                            elif isImmediateToken tok then
                                let immSize = immediateSize tok hasOpSize32 addressSize

                                match tryReadBytes immSize idx bytes with
                                | Some(ni, immBytes) -> loopTokens ni rest (formatImmediate immBytes :: acc)
                                | None -> loopTokens idx rest ("<?..." :: acc)
                            else
                                let expanded =
                                    match tok with
                                    | "rAX"
                                    | "eAX" -> if is32Bit && hasOpSize32 then "EAX" else "AX"
                                    | "rCX"
                                    | "eCX" -> if is32Bit && hasOpSize32 then "ECX" else "CX"
                                    | "rDX"
                                    | "eDX" -> if is32Bit && hasOpSize32 then "EDX" else "DX"
                                    | "rBX"
                                    | "eBX" -> if is32Bit && hasOpSize32 then "EBX" else "BX"
                                    | "rSP"
                                    | "eSP" -> if is32Bit && hasOpSize32 then "ESP" else "SP"
                                    | "rBP"
                                    | "eBP" -> if is32Bit && hasOpSize32 then "EBP" else "BP"
                                    | "rSI"
                                    | "eSI" -> if is32Bit && hasOpSize32 then "ESI" else "SI"
                                    | "rDI"
                                    | "eDI" -> if is32Bit && hasOpSize32 then "EDI" else "DI"
                                    | t when t.Contains('/') -> t.Split('/').[0].Trim()
                                    | t -> t

                                loopTokens idx rest (expanded :: acc)

                    match loopTokens newIdx operands [] with
                    | Some(finalIdx, opStrings) -> Some(finalIdx, effectiveOp.mnemonic, opStrings)
                    | None -> None

    let touchOperation (state: DecoderState) (bytes: byte[]) : (string * int) option =
        if bytes.Length = 0 then
            None
        else
            let rec readPrefixes i prefixes =
                if i >= bytes.Length then
                    i, prefixes
                else
                    let b = bytes[i]

                    if state.prefixSet.Contains b then
                        readPrefixes (i + 1) (b :: prefixes)
                    else
                        i, prefixes

            let startIdx, prefixList = readPrefixes 0 []
            let prefixes = List.rev prefixList

            if startIdx >= bytes.Length then
                None
            else
                let opcodeByte = bytes[startIdx]
                let isTwoByte = opcodeByte = 0x0Fuy

                let hex, startOperandIdx =
                    if isTwoByte && startIdx + 1 < bytes.Length then
                        $"0F{bytes[startIdx + 1]:X2}", startIdx + 2
                    else
                        opcodeByte.ToString("X2"), startIdx + 1

                match Map.tryFind hex state.opcodesMap with
                | None -> if isTwoByte then Some("<?>", 2) else Some("<?>", 1)
                | Some op ->
                    let has66 = prefixes |> List.contains 0x66uy
                    let has67 = prefixes |> List.contains 0x67uy

                    // i386: default operand size is 32-bit, 0x66 toggles to 16-bit
                    // 8086/186/286: default operand size is 16-bit, 0x66 toggles to 32-bit (or ignored)
                    let hasOpSize32 =
                        if state.is32Bit then not has66 else has66

                    // Address size: 32-bit default for i386, 0x67 toggles to 16-bit
                    let addressSize =
                        if state.is32Bit then (if has67 then 16 else 32)
                        else (if has67 then 32 else 16)

                    let hasSeg =
                        prefixes
                        |> List.exists (fun b ->
                            b = 0x26uy || b = 0x2Euy || b = 0x36uy || b = 0x3Euy || b = 0x64uy || b = 0x65uy)

                    let hasRep = prefixes |> List.exists (fun b -> b = 0xF2uy || b = 0xF3uy)
                    let hasLock = prefixes |> List.contains 0xF0uy

                    let parseResult =
                        if op.modRM then
                            resolveModRMOperands state.opcodesMap hex op bytes startOperandIdx hasOpSize32 addressSize state.is32Bit
                        else
                            resolveNoModRMOperands bytes startOperandIdx op.operands hasOpSize32 addressSize state.is32Bit
                            |> Option.map (fun (idx, strs) -> idx, op.mnemonic, strs)

                    match parseResult with
                    | None -> None
                    | Some(newIdx, mnemonic, opStrings) ->
                        let sizeSuffix =
                            if op.operands |> List.exists (fun s -> s = "Yb" || s = "Xb") then
                                "B"
                            elif op.operands |> List.exists (fun s -> s = "Yv" || s = "Xv") then
                                "W"
                            else
                                ""

                        let segPrefix =
                            if hasSeg then
                                let segByte =
                                    prefixes
                                    |> List.find (fun b ->
                                        b = 0x26uy
                                        || b = 0x2Euy
                                        || b = 0x36uy
                                        || b = 0x3Euy
                                        || b = 0x64uy
                                        || b = 0x65uy)

                                match segByte with
                                | 0x26uy -> "ES:"
                                | 0x2Euy -> "CS:"
                                | 0x36uy -> "SS:"
                                | 0x3Euy -> "DS:"
                                | 0x64uy -> "FS:"
                                | 0x65uy -> "GS:"
                                | _ -> ""
                            else
                                ""

                        let repPrefix =
                            if hasRep then
                                (if prefixes |> List.contains 0xF3uy then
                                     "REP "
                                 else
                                     "REPNE ")
                            else
                                ""

                        let lockPrefix = if hasLock then "LOCK " else ""
                        let finalMnemonic = lockPrefix + repPrefix + segPrefix + mnemonic + sizeSuffix
                        let operandsString = String.Join(", ", opStrings |> List.filter ((<>) ""))
                        Some($"{finalMnemonic} {operandsString}".Trim(), newIdx)

    let decode (state: DecoderState) (bytes: byte[]) : DisassembledInstruction list =
        let rec loop offset acc =
            if offset >= bytes.Length then
                List.rev acc
            else
                let slice = bytes[offset..]

                match touchOperation state slice with
                | Some(mnemonic, length) when length > 0 ->
                    let instrBytes =
                        if offset + length <= bytes.Length then
                            bytes[offset .. offset + length - 1]
                        else
                            slice

                    let instr =
                        { Offset = offset
                          Length = length
                          Mnemonic = mnemonic
                          Bytes = instrBytes }

                    loop (offset + length) (instr :: acc)
                | _ ->
                    let instr =
                        { Offset = offset
                          Length = 1
                          Mnemonic = "???"
                          Bytes = [| bytes[offset] |] }

                    loop (offset + 1) (instr :: acc)

        loop 0 []

    let format (instructions: DisassembledInstruction list) =
        instructions
        |> List.map (fun instr ->
            let addr = $"0x%04X{instr.Offset}"
            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " 0x")
            $"{addr}  [0x{bytesStr, -12}]  {instr.Mnemonic}")
    /// <summary>
    /// Uses to resolve next file offset. Recursive decoder resolves current control flow
    /// and starts next control flow since offset returned by
    /// </summary>
    let private rel8Target opcodeOffset instrOffset length (bytes: byte[]) =
        if opcodeOffset + 1 < bytes.Length then
            let rel = sbyte bytes[opcodeOffset + 1] |> int
            [ (instrOffset + length + rel) &&& 0xFFFF ]
        else
            []
    /// <summary>
    /// Uses to resolve next file offset. Recursive decoder resolves current control flow
    /// and starts next control flow since offset returned by
    /// </summary>
    let private rel16Target opcodeOffset instrOffset length (bytes: byte[]) =
        if opcodeOffset + 2 < bytes.Length then
            let rel = BitConverter.ToInt16(bytes, opcodeOffset + 1) |> int
            [ (instrOffset + length + rel) &&& 0xFFFF ]
        else
            []
    /// <summary>
    /// Uses to resolve next file offset for 32-bit relative displacements.
    /// </summary>
    let private rel32Target opcodeOffset instrOffset length (bytes: byte[]) =
        if opcodeOffset + 4 < bytes.Length then
            let rel = BitConverter.ToInt32(bytes, opcodeOffset + 1)
            [ (instrOffset + length + rel) &&& 0xFFFF ]
        else
            []
    /// <summary>
    /// Automatic displacement reader: reads 1, 2 or 4 bytes based on the actual 
    /// displacement size (length after opcode, excluding prefixes).
    /// </summary>
    let private relTarget opcodeOffset instrOffset length (bytes: byte[]) =
        let prefixBytes = opcodeOffset - instrOffset  // number of prefix bytes
        let dispSize = length - prefixBytes - 1       // bytes after the opcode proper
        match dispSize with
        | 1 -> rel8Target opcodeOffset instrOffset length bytes
        | 2 -> rel16Target opcodeOffset instrOffset length bytes
        | 4 -> rel32Target opcodeOffset instrOffset length bytes
        | _ -> []
    /// <summary>
    /// Helps to resolve input arguments for software interrupts. Uses by [resolveInterruptFlow]
    /// </summary>
    let private tryGetAH (bytes: byte[]) (intOffset: int) : byte option =
        // Seek before the interrupt command starts the moving expression (B4 AL, <imm8>) or (B8 AX, <imm16>)
        if intOffset >= 2 && bytes[intOffset - 2] = 0xB4uy then
            Some bytes[intOffset - 1]
        elif intOffset >= 3 && bytes[intOffset - 3] = 0xB8uy then
            Some bytes[intOffset - 1] // little-endian: AL = first byte
        else
            None
    /// <summary>
    /// Depending on loaded interrupt table, disassembler tries to resolve the control flow behavior
    /// For example MS-DOS application part has the following operations:
    /// <code>
    ///     MOV AX, 0x4C01  ; Expected program termination with the (1) exit code
    ///     INT 0x21        ; Call MS-DOS dispatcher. Program will be terminated
    /// </code>
    /// And for this code listing disassembler must drop current control flow.
    /// That's why after exit operation usually follows unreachable code.
    /// </summary>
    let private resolveInterruptFlow
        (state: DecoderState)
        (offset: int)
        (bytes: byte[])
        : FlowControl * int list * string option =
        let intNum = bytes[offset + 1]
        let ahParam = tryGetAH bytes offset

        match Map.tryFind intNum state.interrupts with
        // If no interrupt vectors table loaded or interrupt number not matches
        // Right answer on the question will be "undocumented".
        | None -> SoftwareInterrupt, [], None // без комментария
        | Some intDef ->
            let findArg () =
                intDef.Args
                |> Array.tryFind (fun a ->
                    match a.ArgByte, ahParam with
                    // Star character defines "default case". JSON database of interrupt vectors
                    // always stores arguments of interrupt.
                    | "*", _ -> true
                    | specific, Some ah -> Convert.ToByte(specific, 16) = ah
                    | _ -> false)

            match findArg () with
            | Some arg ->
                let flow = if arg.ReturnFlow then FallThrough else Return
                flow, [], Some arg.Comment
            | None -> SoftwareInterrupt, [], None

    /// <summary>
    /// Find the first non-prefix byte offset within an instruction.
    /// Scans forward from 'startOffset' skipping known prefix bytes.
    /// Returns the offset of the actual opcode byte.
    /// </summary>
    let private skipPrefixes (state: DecoderState) (bytes: byte[]) (startOffset: int) =
        let mutable i = startOffset

        while i < bytes.Length && state.prefixSet.Contains bytes[i] do
            i <- i + 1

        i

    let private classifyFlow
        (state: DecoderState)
        (offset: int)
        (length: int)
        (bytes: byte[])
        : FlowControl * int list * string option =
        let opcodeOffset = skipPrefixes state bytes offset

        if opcodeOffset >= bytes.Length then
            FallThrough, [], None
        else
            let opcode = bytes[opcodeOffset]

            match opcode with
            | 0x0Fuy ->
                let jmpOffset = opcodeOffset + 1

                if jmpOffset < bytes.Length then
                    let b2 = bytes[jmpOffset]

                    if b2 >= 0x80uy && b2 <= 0x8Fuy then
                        let rel =
                            if jmpOffset + 3 < bytes.Length then
                                BitConverter.ToInt32(bytes, jmpOffset + 1)
                            elif jmpOffset + 1 < bytes.Length then
                                BitConverter.ToInt16(bytes, jmpOffset + 1) |> int
                            else
                                0

                        ConditionalJump, [ (offset + length + rel) &&& 0xFFFF ], None
                    else
                        FallThrough, [], None
                else
                    FallThrough, [], None
            | 0xEBuy -> Jump, rel8Target opcodeOffset offset length bytes, None
            | 0xE9uy -> Jump, relTarget opcodeOffset offset length bytes, None  // rel16/rel32
            | 0xE8uy -> Call, relTarget opcodeOffset offset length bytes, None  // rel16/rel32
            | 0xCDuy ->
                let flow, targets, comment = resolveInterruptFlow state opcodeOffset bytes
                flow, targets, comment
            | 0xCEuy -> FallThrough, [], None
            | 0xC2uy
            | 0xC3uy
            | 0xCBuy
            | 0xCFuy -> Return, [], None // C2?
            | 0x70uy
            | 0x71uy
            | 0x72uy
            | 0x73uy
            | 0x74uy
            | 0x75uy
            | 0x76uy
            | 0x77uy
            | 0x78uy
            | 0x79uy
            | 0x7Auy
            | 0x7Buy
            | 0x7Cuy
            | 0x7Duy
            | 0x7Euy
            | 0x7Fuy -> ConditionalJump, rel8Target opcodeOffset offset length bytes, None
            | 0xE0uy
            | 0xE1uy
            | 0xE2uy
            | 0xE3uy -> ConditionalJump, rel8Target opcodeOffset offset length bytes, None
            | 0xFFuy ->
                let modrmOffset = opcodeOffset + 1

                if modrmOffset < bytes.Length then
                    let modrm = bytes[modrmOffset]
                    let reg = (modrm >>> 3) &&& 0b111uy

                    match reg with
                    | 2uy -> IndirectCall, [], None
                    | 4uy -> IndirectJump, [], None
                    | 3uy -> IndirectCallFar, [], None
                    | 5uy -> IndirectJumpFar, [], None
                    | _ -> FallThrough, [], None
                else
                    FallThrough, [], None
            | 0xEAuy -> Jump, [], None
            | 0x9Auy -> Call, [], None
            | _ -> FallThrough, [], None


    let decodeInstruction (state: DecoderState) (bytes: byte[]) (offset: int) : DecodedInstruction option =
        if offset < 0 || offset >= bytes.Length then
            None
        else
            let slice = bytes[offset..]

            match touchOperation state slice with
            | Some(mnemonic, length) when length > 0 ->
                let flow, targets, comment = classifyFlow state offset length bytes
                let instrBytes = bytes[offset .. offset + length - 1]

                Some
                    { Offset = offset
                      Length = length
                      Mnemonic = mnemonic
                      Bytes = instrBytes
                      Flow = flow
                      Targets = targets
                      Comment = comment }
            | _ -> None

    let disassembleRecursive
        (state: DecoderState)
        (bytes: byte[])
        (entryPoints: int[])
        : DecodedInstruction list * ByteStatus[] * Set<int> =
        let status = Array.create bytes.Length ByteStatus.Unknown
        let mutable result = []
        let queue = System.Collections.Generic.Queue<int>(entryPoints)
        let entrySet = Set.ofArray (entryPoints)

        while queue.Count > 0 do
            let offset = queue.Dequeue()

            if offset >= 0 && offset < bytes.Length && status[offset] = ByteStatus.Unknown then
                match decodeInstruction state bytes offset with
                | None -> status[offset] <- ByteStatus.Data
                | Some instr ->
                    for i in 0 .. instr.Length - 1 do
                        let pos = offset + i in

                        if pos < bytes.Length then
                            status[pos] <- ByteStatus.Code

                    result <- instr :: result

                    for t in instr.Targets do
                        if t >= 0 && t < bytes.Length && status[t] = ByteStatus.Unknown then
                            queue.Enqueue(t)

                    let fallThrough = offset + instr.Length

                    let hasFall =
                        match instr.Flow with
                        | FallThrough
                        | ConditionalJump
                        | Call
                        | IndirectCall
                        | IndirectCallFar -> true
                        | _ -> false

                    if
                        hasFall
                        && fallThrough < bytes.Length
                        && status[fallThrough] = ByteStatus.Unknown
                    then
                        queue.Enqueue(fallThrough)

        (List.rev result, status, entrySet)

    let formatWithLabels (instructions: DecodedInstruction list) (bytes: byte[]) (status: ByteStatus[]) (entrySet: Set<int>) =
        let labelSet = instructions |> List.collect (fun i -> i.Targets) |> Set.ofList
        // Collect CALL targets -> these will become function entries
        let callTargets =
            instructions
            |> List.filter (fun i -> i.Flow = Call)
            |> List.collect (fun i -> i.Targets)
            |> Set.ofList

        // Collect RET instructions offsets -> for function end markers
        let retOffsets =
            instructions
            |> List.filter (fun i ->
                match i.Flow with
                | Return -> true
                | _ -> false)
            |> List.map (fun i -> i.Offset)
            |> Set.ofList

        // Sort instructions by offset for proper label placement
        let sortedInstructions = instructions |> List.sortBy (fun i -> i.Offset)

        // Track which offsets are directly preceded by a RET -> to add blank line
        let mutable prevWasRet = false
        let sb = StringBuilder()

        for instr in sortedInstructions do
            // Entry point that is not a call target — mark as p_ but always show "Entry point"
            if entrySet.Contains(instr.Offset) && not (callTargets.Contains(instr.Offset)) then
                if prevWasRet |> not then
                    sb.AppendLine ";" |> ignore
                    sb.AppendLine $"; Entry point at 0x{instr.Offset:X4} (export)" |> ignore
                    sb.AppendLine ";" |> ignore
                sb.AppendLine $"p_0x{instr.Offset:X4}:" |> ignore
                prevWasRet <- false

            // Label (non-call target - jump target)
            elif labelSet.Contains(instr.Offset) && not (callTargets.Contains(instr.Offset)) then
                sb.AppendLine $"__0x{instr.Offset:X4}:" |> ignore

            // Function entry: if this instruction is a CALL target, show a header
            if callTargets.Contains(instr.Offset) then
                if prevWasRet |> not then
                    sb.AppendLine ";" |> ignore
                    sb.AppendLine $"; Procedure at 0x{instr.Offset:X4} instruction offset" |> ignore
                    sb.AppendLine ";" |> ignore
                sb.AppendLine $"p_0x{instr.Offset:X4}:" |> ignore
                prevWasRet <- false

            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " ")

            let comment =
                match instr.Comment with
                | Some c -> $"{c}"
                | None -> ""

            sb.AppendLine $"\t{instr.Mnemonic, -30} ; 0x{instr.Offset:X4} {bytesStr} {comment}"
            |> ignore

            prevWasRet <- retOffsets.Contains(instr.Offset)

            // Function end (line after RETurn statement) 
            if prevWasRet then
                sb.AppendLine $"; returned to previous control flow" |> ignore
                sb.AppendLine() |> ignore

        sb.ToString()

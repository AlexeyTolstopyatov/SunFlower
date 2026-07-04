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
      interrupts: Map<byte, IntelInterrupt> // новое поле
      path: string }

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

    let create (opcodesPath: string) (interruptsPath: string option) =
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
          path = "" }


    let private reg8Names = [| "AL"; "CL"; "DL"; "BL"; "AH"; "CH"; "DH"; "BH" |]
    let private reg16Names = [| "AX"; "CX"; "DX"; "BX"; "SP"; "BP"; "SI"; "DI" |]
    let private segNames = [| "ES"; "CS"; "SS"; "DS"; "FS"; "GS" |]

    let private getEffectiveAddress rm =
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

    let private resolveNoModRMOperands bytes startIdx operands hasOpSize32 addressSize =
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
                | "eAX" -> loop idx rest ("AX" :: acc)
                | "rCX"
                | "eCX" -> loop idx rest ("CX" :: acc)
                | "rDX"
                | "eDX" -> loop idx rest ("DX" :: acc)
                | "rBX"
                | "eBX" -> loop idx rest ("BX" :: acc)
                | "rSP"
                | "eSP" -> loop idx rest ("SP" :: acc)
                | "rBP"
                | "eBP" -> loop idx rest ("BP" :: acc)
                | "rSI"
                | "eSI" -> loop idx rest ("SI" :: acc)
                | "rDI"
                | "eDI" -> loop idx rest ("DI" :: acc)
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
                        let targetAddr = (ni + offset) &&& 0xFFFF
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

                        let targetAddr = (ni + rel) &&& 0xFFFF
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
                | "Fv" -> loop idx rest ("" :: acc)
                | t when t.Contains('/') ->
                    let simple = t.Split('/').[0].Trim()
                    loop idx (simple :: rest) acc
                | other -> loop idx rest (other :: acc)

        loop startIdx operands []

    let private resolveModRMOperands opcodesMap hex defaultOperation bytes startIdx hasOpSize32 addressSize =
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
            let firstToken = operands[0]
            let isByte = firstToken.EndsWith("b")

            let regName (r: byte) =
                let idx = int r
                if isByte then reg8Names[idx] else reg16Names[idx]

            let hasSegReg = effectiveOp.operands |> List.exists (fun t -> t.Contains("S"))
            let regStr = if hasSegReg then segNames[int reg] else regName reg
            let mutable idxAfterRm = idx

            let rmStrOpt =
                if modBits = 3uy then
                    Some(regName rm, idxAfterRm)
                else
                    let baseAddress = getEffectiveAddress rm

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
                                (if (sbyte disp[0]) < 0y then "-" else "+") + formatImmediate disp
                            else
                                ""

                        let rmString = baseAddress + dispStr
                        Some(rmString, idxAfterRm)

            match rmStrOpt with
            | None -> None
            | Some(rmStr, newIdx) ->
                let getOperand (token: string) =
                    if token.Contains("E") then rmStr
                    elif token.Contains("G") then regStr
                    elif token.Contains("S") then regStr
                    elif token.StartsWith("M") then rmStr
                    else token

                let opStrings =
                    operands
                    |> List.mapi (fun i tok ->
                        if i = 0 || i = 1 then
                            getOperand tok
                        else
                            let immBytesCount = immediateSize tok hasOpSize32 addressSize

                            match tryReadBytes immBytesCount newIdx bytes with
                            | None -> "<?..."
                            | Some(ni, immBytes) -> formatImmediate immBytes)

                Some(newIdx, effectiveOp.mnemonic, opStrings)

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
                    let addressSize = if has67 then 32 else 16

                    let hasSeg =
                        prefixes
                        |> List.exists (fun b ->
                            b = 0x26uy || b = 0x2Euy || b = 0x36uy || b = 0x3Euy || b = 0x64uy || b = 0x65uy)

                    let hasRep = prefixes |> List.exists (fun b -> b = 0xF2uy || b = 0xF3uy)
                    let hasLock = prefixes |> List.contains 0xF0uy

                    let parseResult =
                        if op.modRM then
                            resolveModRMOperands state.opcodesMap hex op bytes startOperandIdx has66 addressSize
                        else
                            resolveNoModRMOperands bytes startOperandIdx op.operands has66 addressSize
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
    let private rel8Target offset length (bytes: byte[]) =
        if offset + 1 < bytes.Length then
            let rel = sbyte bytes[offset + 1] |> int
            [ (offset + length + rel) &&& 0xFFFF ]
        else
            []
    /// <summary>
    /// Uses to resolve next file offset. Recursive decoder resolves current control flow
    /// and starts next control flow since offset returned by
    /// </summary>
    let private rel16Target offset length (bytes: byte[]) =
        if offset + 2 < bytes.Length then
            let rel = BitConverter.ToInt16(bytes, offset + 1) |> int
            [ (offset + length + rel) &&& 0xFFFF ]
        else
            []
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
    let private resolveInterruptFlow (state: DecoderState) (offset: int) (bytes: byte[]) : FlowControl * int list * string option =
        let intNum = bytes[offset+1]
        let ahParam = tryGetAH bytes offset
        match Map.tryFind intNum state.interrupts with
        // If no interrupt vectors table loaded or interrupt number not matches
        // Right answer on the question will be "undocumented".
        | None -> SoftwareInterrupt, [], None   // без комментария
        | Some intDef ->
            let findArg () =
                intDef.Args |> Array.tryFind (fun a ->
                    match a.ArgByte, ahParam with
                    // Star character defines "default case". JSON database of interrupt vectors
                    // always stores arguments of interrupt.
                    | "*", _ -> true
                    | specific, Some ah -> Convert.ToByte(specific, 16) = ah
                    | _ -> false)
            match findArg() with
            | Some arg ->
                let flow = if arg.ReturnFlow then FallThrough else Return
                flow, [], Some arg.Comment
            | None -> SoftwareInterrupt, [], None
    let private classifyFlow (state: DecoderState) (offset: int) (length: int) (bytes: byte[]) : FlowControl * int list * string option =
        let opcode = bytes[offset]
        match opcode with
        | 0x0Fuy ->
            if offset+1 < bytes.Length then
                let b2 = bytes[offset+1]
                if b2 >= 0x80uy && b2 <= 0x8Fuy then
                    let rel = if offset+4 < bytes.Length then BitConverter.ToInt32(bytes, offset+2)
                              elif offset+2 < bytes.Length then BitConverter.ToInt16(bytes, offset+2) |> int
                              else 0
                    ConditionalJump, [(offset + length + rel) &&& 0xFFFF], None
                else FallThrough, [], None
            else FallThrough, [], None
        | 0xEBuy -> Jump, rel8Target offset length bytes, None
        | 0xE9uy -> Jump, rel16Target offset length bytes, None
        | 0xE8uy -> Call, rel16Target offset length bytes, None
        | 0xCDuy ->
            let flow, targets, comment = resolveInterruptFlow state offset bytes
            // resolveInterruptFlow returns interrupt comment
            flow, targets, comment
        | 0xCEuy -> FallThrough, [], None
        | 0xC3uy | 0xCBuy | 0xCFuy -> Return, [], None
        | 0x70uy | 0x71uy | 0x72uy | 0x73uy | 0x74uy | 0x75uy | 0x76uy | 0x77uy
        | 0x78uy | 0x79uy | 0x7Auy | 0x7Buy | 0x7Cuy | 0x7Duy | 0x7Euy | 0x7Fuy ->
            ConditionalJump, rel8Target offset length bytes, None
        | 0xE0uy | 0xE1uy | 0xE2uy | 0xE3uy -> ConditionalJump, rel8Target offset length bytes, None
        | 0xFFuy ->
            match if offset+1 < bytes.Length then Some bytes[offset+1] else None with
            | Some modrm ->
                let reg = (modrm >>> 3) &&& 0b111uy
                match reg with
                | 2uy -> IndirectCall, [], None
                | 4uy -> IndirectJump, [], None
                | 3uy -> IndirectCallFar, [], None
                | 5uy -> IndirectJumpFar, [], None
                | _ -> FallThrough, [], None
            | None -> FallThrough, [], None
        | 0xEAuy -> Jump, [], None
        | 0x9Auy -> Call, [], None
        | _ -> FallThrough, [], None


    let decodeInstruction (state: DecoderState) (bytes: byte[]) (offset: int) : DecodedInstruction option =
        if offset < 0 || offset >= bytes.Length then None
        else
            let slice = bytes[offset..]
            match touchOperation state slice with
            | Some(mnemonic, length) when length > 0 ->
                let flow, targets, comment = classifyFlow state offset length bytes
                let instrBytes = bytes[offset..offset+length-1]
                Some { Offset = offset; Length = length; Mnemonic = mnemonic; Bytes = instrBytes;
                       Flow = flow; Targets = targets; Comment = comment }
            | _ -> None

    let disassembleRecursive
        (state: DecoderState)
        (bytes: byte[])
        (entryPoints: int[])
        : DecodedInstruction list * ByteStatus[] =
        let status = Array.create bytes.Length ByteStatus.Unknown
        let mutable result = []
        let queue = System.Collections.Generic.Queue<int>(entryPoints)

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

        (List.rev result, status)

    let formatWithLabels (instructions: DecodedInstruction list) (bytes: byte[]) (status: ByteStatus[]) =
        let labelSet = instructions |> List.collect (fun i -> i.Targets) |> Set.ofList
        let sb = StringBuilder()
        for instr in instructions do
            if labelSet.Contains(instr.Offset) then
                sb.AppendLine($"__label_0x{instr.Offset:X4}:") |> ignore
            
            // let byteStatus =
            //     if instr.Length > 0 && instr.Offset < status.Length then
            //         match status[instr.Offset] with
            //         | ByteStatus.Code -> "Code"
            //         | ByteStatus.Data -> "Data"
            //         | _ -> "?"
            //     else "?"
            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " ")
            
            let comment = 
                match instr.Comment with
                | Some c -> $"{c}"
                | None -> ""
            sb.AppendLine($"\t{instr.Mnemonic,-30} # 0x{instr.Offset:X4}|{bytesStr}|{comment}") |> ignore
        sb.ToString()

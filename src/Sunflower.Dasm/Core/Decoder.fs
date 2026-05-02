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
namespace Sunflower.Dasm

open System
open System.IO
open System.Text.Json
open Sunflower.Dasm

type DecoderState =
    { opcodesMap: Map<string, Operation>
      prefixSet: Set<byte>
      path: string }

type Instruction =
    { Opcode: string
      Reg: Nullable<int>
      Mnemonic: string
      Operands: List<string>
      ModRM: bool
      IsPrefix: bool }

type DisassembledInstruction = {
    Offset: int
    Length: int
    Mnemonic: string
    Bytes: byte[]
}

module internal Decoder =
    /// Tries to read database from JSON and casts it into opcodes map
    /// <param name="jsonPath"></param>
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
            // Define: If given operation is pure or having special group key
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
            | false, _ -> op // group key found. Don't touch it
        )
        |> Array.fold (fun acc op -> Map.add op.code op acc) Map.empty

    let create (opcodesPath: string) =
        if not (File.Exists opcodesPath) then
            failwith "Can't find Intel 8086 opcodes map"

        let allOps = loadOpcodeMap (opcodesPath)

        let prefixes =
            allOps
            |> Map.filter (fun _ op -> op.prefix)
            |> Map.keys
            |> Seq.map (fun hex -> Convert.ToByte(hex, 16))
            |> Set.ofSeq

        { opcodesMap = allOps
          prefixSet = prefixes
          path = "" }

    /// 8-bit general purpose registers definitions
    let private reg8Names = [| "AL"; "CL"; "DL"; "BL"; "AH"; "CH"; "DH"; "BH" |]
    /// 16-bit general purpose registers definitions
    let private reg16Names = [| "AX"; "CX"; "DX"; "BX"; "SP"; "BP"; "SI"; "DI" |]
    /// Segment registers definition
    let private segNames = [| "ES"; "CS"; "SS"; "DS"; "FS"; "GS" |] // 0..5

    /// Matches and returns pointer depending on register/memory byte
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
        | "Ib" // Immediate .BYTE
        | "Jb" -> 1 //
        | "Iw" -> 2 // Immediate .WORD
        | "Id" -> 4 // Immediate .DWORD
        | "Iz"
        | "Iv"
        | "Jz" -> if hasOpSize32 then 4 else 2
        | "Ob"
        | "Ov" -> if addressSize = 32 then 4 else 2
        | "Mp"
        | "Ap"
        | "p" -> 4 // .FAR
        | t -> failwithf $"Unsupported immediate token: %s{t}"

    let private tryReadByte (i: int) (bytes: byte[]) : (int * byte) option =
        if i < bytes.Length then Some(i + 1, bytes[i]) else None

    let private tryReadBytes (n: int) (i: int) (bytes: byte[]) : (int * byte[]) option =
        if i + n <= bytes.Length then
            Some(i + n, bytes[i .. i + n - 1])
        else
            None

    /// Resolves immediate operation code without mod R/M
    /// Returns optional tuple of immediate (instruction and #)
    /// <param name="bytes">operation code bytes sequence</param>
    /// <param name="startIdx">where the instruction starts</param>
    /// <param name="operands">operands</param>
    /// <param name="hasOpSize32">byte/word sized operand</param>
    /// <param name="addressSize">operator size</param>
    let private resolveNoModRMOperands
        (bytes: byte[])
        (startIdx: int)
        (operands: string list)
        (hasOpSize32: bool)
        (addressSize: int)
        =
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
                    | Some(ni, bs) ->
                        let addr = formatImmediate bs
                        loop ni rest ($"[%s{addr}]" :: acc)
                | "Xb"
                | "Yb"
                | "Xv"
                | "Yv"
                | "Fv" -> // no thoughts. head empty
                    loop idx rest ("" :: acc)
                | t when t.Contains('/') ->
                    let simple = t.Split('/').[0].Trim()
                    loop idx (simple :: rest) acc
                | other -> loop idx rest (other :: acc)

        loop startIdx operands []

    /// Resolves ModR/M bytes sequence and returns tuple what is presenting (new#, operands<string>)
    /// <param name="startIdx">Where is the modr/m starts</param>
    /// <param name="defaultOp">source might be a group</param>
    let private resolveModRMOperands
        (opcodesMap: Map<string, Operation>)
        (hex: string)
        (defaultOperation: Operation)
        (bytes: byte[])
        (startIdx: int)
        (hasOpSize32: bool)
        (addressSize: int)
        : (int * string * string list) option =
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
            // let regSize = if isByte then 1 else 2

            let regName (r: byte) =
                let idx = int r
                if isByte then reg8Names[idx] else reg16Names[idx]

            let hasSegReg = effectiveOp.operands |> List.exists (fun t -> t.Contains("S"))
            let regStr =
                if hasSegReg then
                    segNames[int reg]
                else
                    regName reg // AL/AX...
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
                                let sign = if (sbyte disp[0]) < 0y then "-" else "+"
                                $"%s{sign}%s{formatImmediate disp}"
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
                // And now i'm expecting Read bytes count, mnemonic, operand strings
                Some(newIdx, effectiveOp.mnemonic, opStrings)

    let touchOperation (state: DecoderState) (bytes: byte[]) : (string * int) option =
        if bytes.Length = 0 then
            None
        else
            /// Recursively iterates each prefix in the record
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
                let hex = opcodeByte.ToString("X2")

                match Map.tryFind hex state.opcodesMap with
                | None -> Some("<?>", 1) // Unknown operator, known size.
                | Some op ->
                    let has66 = prefixes |> List.contains 0x66uy
                    let has67 = prefixes |> List.contains 0x67uy
                    let addressSize = if has67 then 32 else 16

                    let hasSeg =
                        prefixes
                        |> List.exists (fun b ->
                            b = 0x26uy || b = 0x2Euy || b = 0x36uy || b = 0x3Euy || b = 0x64uy || b = 0x65uy)

                    let hasRep = prefixes |> List.exists (fun b -> b = 0xF2uy || b = 0xF3uy) // prefixes |> List.contains 0xF3uy
                    let hasLock = prefixes |> List.contains 0xF0uy

                    let parseResult =
                        let startOperandIdx = startIdx + 1

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

                        /// Collect mnemonics with resolved prefixes
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
                        let totalBytes = newIdx // Points to the next byte
                        Some($"{finalMnemonic} {operandsString}".Trim(), totalBytes)
    
    let decode (state: DecoderState) (bytes: byte[]) : DisassembledInstruction list =
        let rec loop offset acc =
            if offset >= bytes.Length then
                List.rev acc
            else
                let slice = bytes[offset..]
                match touchOperation state slice with
                | Some (mnemonic, length) when length > 0 ->
                    let instrBytes = if offset + length <= bytes.Length then bytes[offset .. offset + length - 1] else slice
                    let instr = { Offset = offset; Length = length; Mnemonic = mnemonic; Bytes = instrBytes }
                    loop (offset + length) (instr :: acc)
                | _ ->
                    // Bad result. Can't disassemble.
                    let instr = { Offset = offset; Length = 1; Mnemonic = "???"; Bytes = [| bytes[offset] |] }
                    loop (offset + 1) (instr :: acc)
        loop 0 []
        
    let format (instructions: DisassembledInstruction list) =
        instructions |> List.map (fun instr ->
            let addr = $"0x%04X{instr.Offset}"
            let bytesStr = BitConverter.ToString(instr.Bytes).Replace("-", " 0x")
            $"{addr}  [0x{bytesStr,-12}]  {instr.Mnemonic}"
        )
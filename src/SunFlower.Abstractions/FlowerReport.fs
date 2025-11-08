namespace SunFlower.Abstractions

open System
open System.Text
// SunFlower datatypes declared like machine word sizes
//      :1 | BYTE   | byte/sbyte   | 
//      :2 | WORD   | UInt16/Int16 | 
//      :4 | DWORD  | UInt32/Int32 | 
//      :8 | QWORD  | UInt64/Int64 |
//      :s | <'Any> | String       | Any type of string (e.g. NET String)
//      :s_| BYTE[] | Char[]       | NON-Terminated ASCII string
//      :sz| BYTE[] | Char[]       | Terminated ASCII String
//      :ps| BYTE[] | Char[]       | Pascal String
//      :bs| WORD[] | String       | Binary String [OR] UTF-16 .NET String
//      :f | BYTE   | Boolean      | Flag
//
// Column (type) declaration:
//      FlowerReport.ForColumn("lpExternalTable", typeof(int)) -> "lpExternalTable:4"
type FlowerType =
    | U1 = 1
    | U2 = 2
    | U4 = 4
    | U8 = 8
    | ASCII = 5
    | CStr = 3
    | BStr = 11
    | WStr = 13
    | PascalStr = 7
    | AnyStr = 9
    | Flag = 10

module FlowerReport =
    /// <summary>
    /// Shields raw ASCII string.
    /// Replaces ASCII bytes "\xx" substrings instead  
    /// </summary>
    /// <param name="str">Unsafe CLR string given from ASCII</param>
    [<CompiledName "SafeString">]
    let safe_string (str: string) : string =
        let sb = StringBuilder()
        
        let appendEscaped (b: byte) =
            match b with
            | 0uy -> sb.Append("%0")
            | 9uy -> sb.Append(@"%t")
            | 10uy -> sb.Append(@"%n")
            | 13uy -> sb.Append(@"%r")
            | 92uy -> sb.Append(@"\\") // <-- backslash warning
            | _ when b >= 32uy && b <= 126uy ->
                sb.Append(char b)
            | _ -> 
                sb.AppendFormat(@$"%%x{b:X2}")

        str
        |> Encoding.ASCII.GetBytes
        |> Array.iter (appendEscaped >> ignore)
        
        $"`{sb}`"
    [<CompiledName "ForColumn">]
    let for_column (str : string, t : Type) : string =
        match t.Name with
        | "Int32"
        | "UInt32" -> $"`{str}:4`"
        | "Int16"
        | "UInt16" -> $"`{str}:2`"
        | "Byte"
        | "SByte" -> $"`{str}:1`"
        | "Object"
        | "String" -> $"`{str}:s`" // <-- I don't know type of string what become UTF-16
        | "Boolean" -> $"`{str}:f`"
        | _ -> $"`{str}:?`"
    /// <summary>
    /// Returns the following string format
    /// name:type
    /// </summary>
    [<CompiledName "ForColumnStr">]
    let for_column_str (str: string, t : string) : string = 
        $"`{str}:{t}`"
    [<CompiledName "ForColumnFl">]
    let for_column_fl (str: string, t: FlowerType) =
        match t with
        | FlowerType.U1 -> $"`{str}:1`"
        | FlowerType.U2 -> $"`{str}:2`"
        | FlowerType.U4 -> $"`{str}:4`"
        | FlowerType.U8 -> $"`{str}:8`"
        | FlowerType.ASCII -> $"`{str}:s_`"
        | FlowerType.CStr -> $"`{str}:sz`"
        | FlowerType.PascalStr -> $"`{str}:ps`"
        | FlowerType.BStr -> $"`{str}:bs`"
        | FlowerType.AnyStr -> $"`{str}:s`"
        | FlowerType.Flag -> $"`{str}:f`"
        | _ -> $"`{str}:?`"
        
    
    /// <summary>
    /// Returns prepared string of FAR16 pointer
    /// </summary>
    [<CompiledName "FarHexString">]
    let far_hex_string(seg: UInt16, offset: UInt16, high: bool) : string = 
        match high with
        | true -> $"`{seg:X4}:{offset:X4}`"
        | false -> $"`{offset:X4}:{seg:X4}`"
    /// <summary>
    /// Returns prepared string of FAR32 pointer
    /// </summary>
    [<CompiledName "Far32HexString">]
    let far32_hex_string (seg: UInt16, offset: UInt32, high: bool) = 
        match high with
        | true -> $"`{seg:X4}:{offset:X8}`"
        | false -> $"`{offset:X8}:{seg:X4}`"
    
    /// <summary>
    /// Machine word retranslates to hexadecimal view like 0xDEADBABE
    /// with fixed byte-size using
    /// </summary>
    [<CompiledName "WordToString">]
    let word_to_string (v: string, size: Int32) : string = 
        let w = Convert.ToUInt64 v
        match size with
        | 1 -> $"0x{w:X2}"
        | 2 -> $"0x{w:X4}"
        | 4 -> $"0x{w:X8}"
        | _ -> $"0x{w:X}"


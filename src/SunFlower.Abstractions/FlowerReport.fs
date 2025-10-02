namespace SunFlower.Abstractions

open System
open System.Text

module FlowerReport =
    /// <summary>
    /// Shields raw ASCII string.
    /// Replaces ASCII bytes "\xx" substrings instead  
    /// </summary>
    /// <param name="str">Unsafe CLR string given from ASCII</param>
    [<CompiledName "SafeString">]
    let safeString (str: string) : string =
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
    let forColumn (str : string, t : Type) : string =
        match t.Name with
        | "Int32"
        | "UInt32" -> $"`{str}:4`"
        | "Int16"
        | "UInt16" -> $"`{str}:2`"
        | "Byte"
        | "SByte" -> $"`{str}:1`"
        | "Object"
        | "String" -> $"`{str}:s`"
        | "Boolean" -> $"`{str}:f`"
        | _ -> $"`{str}:?`"
    /// <summary>
    /// Returns the following string format
    /// name:type
    /// </summary>
    [<CompiledName "ForColumnWith">]
    let forColumnWith (str: string, t : string) : string = 
        $"`{str}:{t}`"
    /// <summary>
    /// Returns prepared string of FAR16 pointer
    /// </summary>
    [<CompiledName "FarHexString">]
    let farHexString(seg: UInt16, offset: UInt16, high: bool) : string = 
        match high with
        | true -> $"`{seg:X4}:{offset:X4}`"
        | false -> $"`{offset:X4}:{seg:X4}`"
    /// <summary>
    /// Returns prepared string of FAR32 pointer
    /// </summary>
    [<CompiledName "Far32HexString">]
    let far32HexString (seg: UInt16, offset: UInt32, high: bool) = 
        match high with
        | true -> $"`{seg:X4}:{offset:X8}`"
        | false -> $"`{offset:X8}:{seg:X4}`"

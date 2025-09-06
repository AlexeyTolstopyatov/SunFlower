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
            | 0uy -> sb.Append(@"\{0}")
            | 9uy -> sb.Append(@"\{t}")
            | 10uy -> sb.Append(@"\{n}")
            | 13uy -> sb.Append(@"\{r}")
            | 92uy -> sb.Append(@"\\") // <-- backslash warning
            | _ when b >= 32uy && b <= 126uy ->
                sb.Append(char b)
            | _ -> 
                sb.AppendFormat(@"\{0:X2}", b)

        str
        |> Encoding.ASCII.GetBytes
        |> Array.iter (appendEscaped >> ignore)
        
        $"`{sb}`"
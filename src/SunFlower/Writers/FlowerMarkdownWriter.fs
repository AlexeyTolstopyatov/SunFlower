//
// CoffeeLake (C) 2024-2026
// This module represents "markdown" syntax writer API
// To describe records what were made by external sunflower plugin
// into the text document.
//
// @creator: atolstopyatov2017@vk.com
//   
module SunFlower.Writers.FlowerMarkdownWriter

open System
open System.Collections.Generic
open System.Data
open Microsoft.FSharp.Core
open SunFlower.Abstractions.Types

// Module abstract ends. Result of IFlowerSeed interface into collection of results
// supports some casts:
//  - Strings       IEnumerable<string>
//  - Bytes         Byte[]
//  - Region        SunFlower.Abstractions.Types.Region <-- My type :3
//                  (Header, Content, Table)
//  - DataTable     System.Data.DataTable
// Support of "Type -> Text" converters will be implemented here

/// <summary>
/// Smart cast into text
/// Supports:
///  - IEnumerable{string} derived types
///  - String
/// </summary>
[<CompiledName "FormatStrings">]
let format_str(lines: obj): string =
   match lines with
    | :? IEnumerable<String> as e -> "\n" + String.Join("\n", e) + "\n"
    | :? String as s -> s
    | _ -> $"\n`bad cast of this line {lines.GetType()}`\n"
/// <summary>
/// Cast of DataTable into Markdown table for the document
/// NULL-contained fields will be empty strings-paddings in the table 
/// </summary>
/// <param name="table">target table</param>
[<CompiledName "FormatTable">]
let format_table(table: DataTable): string =
    let buffer: List<string> = List<string>()
    
    let safeToString (value: obj) =
        if Convert.IsDBNull(value) then
            " " // Empty cell (<null> dont need anymore)
        else
            $"%O{value}"
    let add(s: string) =
        buffer.Add s
    let rows = table.Rows |> Seq.cast<DataRow>
    let columns = table.Columns |> Seq.cast<DataColumn>
    
    let columnWidths =
        columns
        |> Seq.map (fun col ->
            let headerWidth = col.ColumnName.Length

            let contentWidth =
                rows
                |> Seq.map (fun row -> safeToString row[col] |> String.length)
                |> Seq.append [ headerWidth ]
                |> Seq.max

            (col.Ordinal, (contentWidth + 2)))
        |> Map.ofSeq
    // Draw Headers
    // | Head:s | Example:sz |
    columns
        |> Seq.map (fun col -> col.ColumnName.PadRight(columnWidths[col.Ordinal] - 2))
        |> String.concat " | "
        |> sprintf "| %s |"
        |> add
    // |--------|------------|
    columns
        |> Seq.map (fun col -> String('-', columnWidths[col.Ordinal] - 2))
        |> String.concat "-|-"
        |> sprintf "|-%s-|"
        |> add
    // Draw values
    rows
        |> Seq.iter (fun row ->
            columns
            |> Seq.map (fun col -> row[col] |> safeToString |> _.PadRight(columnWidths[col.Ordinal] - 2))
            |> String.concat " | "
            |> sprintf "| %s |"
            |> add)

    String.Join('\n', buffer)
    
/// <summary>
/// Returns sum of strings divided by new line escape character
/// <example language="FSharp">
/// <code>
/// let content = "Let's build a markdown list"
///     |+ " - Use operator reloads"
///     |+ " - Don't forget about global defined operators"
///     |+ " - hihi"
/// </code>
/// </example>
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
let private (|+) (a: String) (b: String) =
   a + b + "\n"
    
/// <summary>
/// Makes a simple "Papers Section". The Region container
/// contains Header, Content what describes the object
/// and table what helps to analyse this object.
/// </summary>
/// <param name="reg">Given by FlowerResult collection unboxed result</param>
[<CompiledName "FormatRegion">]
let format_reg(reg: Region) =
    ""
        |+ reg.Head
        |+ reg.Content
        |+ format_table reg.Table
        |+ "\n"
// **Remove the format_reg in 4.5.1+ releases** \\ 
/// <summary>
/// Makes a simple "Papers Section". The region container
/// contains Header, Content, chat must to describe target
/// object and table what represents you information about this target.
/// </summary>
/// <param name="reg">Current region given+unboxed from FlowerResult collection</param>
/// <param name="header_level">Markdown Heading level</param>
[<CompiledName "FormatRegionSmartHeader">]
let format_reg2 (reg: Region, header_level: int) =
    "#"
        |> String.replicate header_level // <-- Header declaration must be separated by the value 
        |+ $" {reg.Head}"
        |+ reg.Content
        |+ format_table reg.Table
        |+ "\n"
/// <summary>
/// Transforms all Regions collection into one Markdown written next 
/// 
/// Warning: The Writers algorithm will be rewritten next time
/// Reason: FlowerResults contains boxed values and unboxing mechanism
/// couldn't be one-typed. (FlowerResult collection can contain different
/// typed entries)
/// <example>
/// <code>
/// [Strings, Bytes, Region, Region, Strings]
/// </code>
/// </example>
/// </summary>
/// <param name="list"></param>
[<CompiledName "FormatRegions">]
let rec format_regs(list: 'TIteratable) = 
    
    0

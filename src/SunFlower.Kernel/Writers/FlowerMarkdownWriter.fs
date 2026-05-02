//
// CoffeeLake (C) 2024-2026
// This module represents "markdown" syntax writer API
// To describe records what were made by external sunflower plugin
// into the text document.
//
// @creator: atolstopyatov2017@vk.com
//
module SunFlower.Kernel.Writers.FlowerMarkdownWriter

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
let formatStrings (lines: obj) : string =
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
let formatTable (table: DataTable) : string =
    let buffer: List<string> = List<string>()

    let safeToString (value: obj) =
        if Convert.IsDBNull(value) then
            " " // Empty cell (<null> dont need anymore)
        else
            $"%O{value}"

    let add (s: string) = buffer.Add s
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
///     |+ " - Item1"
///     |+ " - Item2"
///     |+ " - Item3"
/// {EOL}
/// </code>
/// This variable stores string:
/// <code>
/// Let's build a markdown list
///  - Item1
///  - Item2
///  - Item3
/// </code>
/// </example>
/// </summary>
/// <param name="a"></param>
/// <param name="b"></param>
let private (|+) (a: String) (b: String) = a + b + "\n"

/// <summary>
/// Makes a simple "Papers Section". The region container
/// contains Header, Content, chat must describe target
/// object and table what represents you information about this target.
/// </summary>
/// <param name="reg">Current region given+unboxed from FlowerResult collection</param>
/// <param name="header_level">Markdown Heading level</param>
[<CompiledName "FormatRegion">]
let formatRegion (reg: Region) (header_level: int) =
    "#" |> String.replicate header_level // <-- Header declaration must be separated by the value
    |+ $" {reg.Head}"
    |+ reg.Content
    |+ formatTable reg.Table
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
let formatRegions (list: IEnumerable<Region>) =
    let regs = list |> Seq.map (fun reg -> formatRegion reg 3)

    String.Join("\n", regs) |+ "\n"

/// <summary>
/// Transforms all Regions collection into one Markdown written next
///
/// Warning: The Writers algorithm will be rewritten next time
/// Reason: FlowerResults contains boxed values and unboxing mechanism
/// couldn't be one-typed. (FlowerResult collection can contain different
/// typed entries)
/// </summary>
/// <param name="list"></param>
[<CompiledName "FormatTables">]
let formatTables (list: IEnumerable<DataTable>) =
    let regs = list
               |> Seq.map formatTable

    String.Join("\n", regs) |+ "\n"

[<CompiledName "Write">]
let write (results: IEnumerable<FlowerSeedResult>) =
    let mutable acc = "" |+ $"Generated at: {DateTime.Now}"

    for i in results do
        match i.BoxedResult with
        | :? IEnumerable<Region> as regs -> acc <- acc |+ formatRegions regs |+ "\n"
        | :? IEnumerable<String> as strs -> acc <- acc |+ formatStrings strs |+ "\n"
        | :? IEnumerable<DataTable> as dts -> acc <- acc |+ formatTables dts |+ "\n"
        | unknown -> acc <- acc |+ $"```\n{unknown.GetType}\n```\n\n"

    acc

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

    columns
        |> Seq.map (fun col -> col.ColumnName.PadRight(columnWidths[col.Ordinal] - 2))
        |> String.concat " | "
        |> sprintf "| %s |"
        |> add
    
    columns
        |> Seq.map (fun col -> String('-', columnWidths[col.Ordinal] - 2))
        |> String.concat "-|-"
        |> sprintf "|-%s-|"
        |> add
    
    rows
        |> Seq.iter (fun row ->
            columns
            |> Seq.map (fun col -> row[col] |> safeToString |> _.PadRight(columnWidths[col.Ordinal] - 2))
            |> String.concat " | "
            |> sprintf "| %s |"
            |> add)

    String.Join('\n', buffer)


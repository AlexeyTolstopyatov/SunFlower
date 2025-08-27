namespace SunFlower.Terminal

open System
open System.Collections.Generic
open System.Data
open System.IO
open System.Net
open SunFlower.Services

//
// CoffeeLake (C) 2024-2025
// This module particular belongs JellyBins
// Licensed under MIT
//
// @creator: atolstopyatov2017@vk.com
//

/// <summary>
/// Module represents functional for printing
/// <see cref="DataTable"/>s, <see cref="Dictionary"/>
/// structures and will be extended later
/// </summary>
module DataView =
    /// <summary>
    /// Prints dictionary as table without delimiters
    /// </summary>
    /// <param name="d"></param>
    let printDictionary (d: Dictionary<string, string>) : unit =
        for KeyValue(k, v) in d do
            printfn $"{k}\t{v}"
    /// Prepares and prints <see cref="DataTable"/> instance
    /// as Markdown Table
    /// </summary>
    /// <param name="table"></param>
    let printDataTable (table: DataTable) =
        let safeToString (value: obj) =
            if Convert.IsDBNull(value) then
                " " // Empty cell (<null> dont need anymore)
            else
                $"%O{value}"

        let rows = table.Rows
                    |> Seq.cast<DataRow>
        let columns = table.Columns
                    |> Seq.cast<DataColumn>

        let columnWidths =
            columns
            |> Seq.map (fun col ->
                let headerWidth = col.ColumnName.Length
                let contentWidth = 
                    rows
                    |> Seq.map (fun row -> safeToString row[col] |> String.length)
                    |> Seq.append [headerWidth]
                    |> Seq.max
                (col.Ordinal, (contentWidth + 2))
            )
            |> Map.ofSeq

        let _formatString =
            columns
            |> Seq.map (fun col -> 
                let width = columnWidths[col.Ordinal] - 4
                sprintf "| %%-%ds " width 
            )
            |> String.concat ""
            |> fun s -> s + "|"
            
        columns
            |> Seq.map (fun col -> col.ColumnName.PadRight(columnWidths[col.Ordinal] - 2))
            |> String.concat " | "
            |> printfn "| %s |"

        columns
            |> Seq.map (fun col -> String('-', columnWidths[col.Ordinal] - 2))
            |> String.concat "-|-"
            |> printfn "|-%s-|"

        rows
            |> Seq.iter (fun row ->
                columns
                |> Seq.map (fun col ->
                    row[col]
                    |> safeToString
                    |> _.PadRight(columnWidths[col.Ordinal] - 2)
                )
                |> String.concat " | "
                |> printfn "| %s |"
            )
        printfn ""
    ()

module App =
    type Command =
    | CheckSingle of path: string
    | ExplainSingle of path: string
    | Explain
    | CheckAll
    | Help
    | Invalid of message: string

    let parseArgs (args: string[]) =
        match args with
        | [| "--for"; path |] -> CheckSingle path
        | [| "--forall" |] -> CheckAll
        | [| "--why"; path |] -> ExplainSingle path
        | [| "--help" |] | [| "-h" |] | [| "/?" |] -> Help
        | [| "--what" |] | [| "--about" |] -> Explain
        | [||] -> Help 
        | _ -> Invalid "Unknown key"

    let showHelp () =
        printfn "Usage:"
        printfn "  --for <path>    Compare current sunflower plugin with base"
        printfn "  --forall        Compare all plugins with base"
        printfn "  --why <path>    Compare current sunflower plugin verbose"
        printfn "  --help, -h, /?  Show this page"

    let executeCommand command =
        match command with
        | CheckSingle path ->
            let result = FlowerCompatibility.get path
            DataView.printDataTable result
        | CheckAll ->
            let result = FlowerCompatibility.getForAll ()
            DataView.printDataTable result
        | ExplainSingle path ->
            let result = FlowerCompatibility.getVerbose path
                         |> Seq.toList
                         |> List.iter (printfn "%s")
            result
            ()
        | Explain ->
            try
                let result = File.ReadAllText (Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SunFlower.runtimeconfig.dll"))
                printfn "%s" result
            with
            | ex -> printfn "Couldn't find resources"
        | Help ->
            showHelp ()
        | Invalid message ->
            printfn "Error: %s" message
            showHelp ()
    
    [<EntryPoint>]
    let main (args: string[]) =
        let command = parseArgs args
        executeCommand command
        0
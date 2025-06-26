namespace SunFlower.Terminal

open System
open System.Collections.Generic
open System.Data
open System.Diagnostics
open System.IO
open System.Reflection
open SunFlower.Abstractions

///
/// CoffeeLake (C) 2024-2025
/// This module belongs JellyBins
/// Licensed under MIT
///
/// Module represents functional for printing
/// <see cref="DataTable"/>s, <see cref="Dictionary"/>
/// structures and will be extended later
/// 
module DataView =
    /// <summary>
    /// Prints dictionary as table without delimeters
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
///
/// CoffeeLake (C) 2025
///
/// GUI-less application for static binary analysis
/// includes Sunflower loader API (see SunFlower.FlowerSeedManager)
///
/// Provides the ability to call external
/// libraries compatible with the IFlowerSeed
/// interface for further user intentions.
///
module UserInterface =
    
    type AppState = {
        Plugins: IFlowerSeed list
        ActivePlugins: IFlowerSeed list
        CurrentFile: string option
    }
    
    let showMainMenu state =
        Console.Clear()
        let opt = Option.defaultValue "NONE" state.CurrentFile
        
        Console.ForegroundColor <- ConsoleColor.Yellow
        printfn $"--- Required foundation build: %s{FileVersionInfo.GetVersionInfo(Assembly.GetCallingAssembly().Location).FileVersion}"
        Console.ResetColor()
        
        
        printfn "=== SunFlower Firmware Analyzer ==="
        printfn $"File: %s{opt}" // !!!
        printfn $"Active plugins: {state.ActivePlugins.Length}"
        printfn "----------------------------------------"
        printfn "[A] Analyze file"
        printfn "[P] Select plugins"
        printfn "[F] Select file"
        printfn "[R] Show results"
        printfn "[Q] Quit"
        printfn "----------------------------------------"
        printf "Select option: "
    
    let showPluginSelection (plugins: IFlowerSeed list) (activePlugins: IFlowerSeed list) =
        Console.Clear()
        printfn "=== Seeds ==="
        
        printfn "0. Select all"
        
        plugins
        |> List.iteri (fun i plugin ->
            let status = if List.contains plugin activePlugins then "[X]" else "[ ]"
            printfn $"{i+1}. {status} {plugin.Seed}"
        )
        
        printf "\nEnter plugin numbers (comma separated): "
        Console.ReadLine()
    
    let showAnalysisResults (results: (string * FlowerSeedStatus) list) =
        Console.Clear()
        
        results
        |> List.iter (fun (name, result) ->
            Console.ForegroundColor <- 
                if result.IsEnabled then ConsoleColor.Green 
                else ConsoleColor.Yellow
            
            printfn $"-> {name}"
            
            if not result.IsEnabled then
                printfn "load failed"
                printfn "\n---Seed Exceptions chain starts---"
                printfn $"{result.LastError}"
                printfn "---Seed Exception chains ends---"
            elif result.Result = null || result.Result.Length = 0 then
                printfn "no result"
            else // <-- plugin has results
                result.Result
                |> Array.iter (fun table ->
                    printfn $"\n{table.TableName.ToUpper()}"
                    DataView.printDataTable table
                )
        )
        
        printfn "\nPress any key to continue..."
        Console.ReadKey() |> ignore

open UserInterface
module AnalysisEngine =
    let analyzeFile (plugins: IFlowerSeed list) filePath =
        printfn $"Processing %s{ FileInfo(filePath).Name }"
        
        plugins
        |> List.map (fun plugin ->
            async {
                try
                    printfn $"Calling plugin: {plugin.Seed}"
                    plugin.Main(filePath) |> ignore // just call it 
                    return (plugin.Seed, plugin.Status)
                with ex ->
                    return (plugin.Seed, FlowerSeedStatus(
                        IsEnabled = false,
                        Result = [||],
                        LastError = ex
                    ))
            }
        )
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.toList

    let rec mainLoop (state: AppState) =
        showMainMenu state
        
        match Console.ReadKey(true).KeyChar with
        | 'a' | 'A' -> 
            match state.CurrentFile, state.ActivePlugins with
            | Some file, plugins when not (List.isEmpty plugins) ->
                let results = analyzeFile plugins file
                showAnalysisResults results
                mainLoop state
            | None, _ ->
                printfn "No file selected! \nPress any key..."
                Console.ReadKey() |> ignore
                mainLoop state
            | _, _ ->
                printfn "SunFlower seeds selected \nPress any key..."
                Console.ReadKey() |> ignore
                mainLoop state
                
        | 'p' | 'P' ->
            let input = showPluginSelection 
                            (state.Plugins |> Seq.toList) 
                            (state.ActivePlugins |> Seq.toList)
            
            let newActivePlugins =
                if input = "0" then
                    state.Plugins |> Seq.toList
                else
                    input.Split(',')
                    |> Array.choose (fun s -> 
                        match Int32.TryParse(s.Trim()) with
                        | true, num when num > 0 && num <= state.Plugins.Length -> 
                            Some state.Plugins.[num - 1]
                        | _ -> None)
                    |> Array.toList
            
            mainLoop { state with ActivePlugins = newActivePlugins }
        
        | 'f' | 'F' ->
            Console.Clear()
            printf "Enter file path: "
            let path = Console.ReadLine()
            
            if File.Exists path then
                mainLoop { state with CurrentFile = Some path }
            else
                printfn "File not found! \nPress any key..."
                Console.ReadKey() |> ignore
                mainLoop state
        
        | 'r' | 'R' ->
            match state.CurrentFile, state.ActivePlugins with
            | Some file, plugins when not (List.isEmpty plugins) ->
                let results = analyzeFile plugins file
                showAnalysisResults results
                mainLoop state
            | _ -> mainLoop state
        
        | 'q' | 'Q' -> ()
        | _ -> mainLoop state


module App =
    [<EntryPoint>]
    let main (args: string[]):int =
        printfn "--- Collecting seeds"
        
        let plugins = SunFlower.Services.FlowerSeedManager
                              .CreateInstance()
                              .LoadAllFlowerSeeds()
                              .Seeds
                              |> Seq.toList
        
        let initialState = {
            Plugins = plugins
            ActivePlugins = plugins
            CurrentFile = 
                if args.Length > 0 && File.Exists args[0] 
                then Some args[0] 
                else None
        }
        printfn $"--- Initial Tracing"
        printfn $"{initialState}"
        
        #if DEBUG
        Console.Write("Press any key to continue . . .")
        Console.ReadKey() |> ignore
        #endif
        
        AnalysisEngine.mainLoop initialState
        0
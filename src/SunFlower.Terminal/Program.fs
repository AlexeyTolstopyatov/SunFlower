namespace SunFlower.Terminal

open System
open System.Data
open System.Diagnostics
open System.IO
open System.Reflection
open SunFlower.Abstractions

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
            
            printfn $"--- {name}"
            
            if not result.IsEnabled then
                printfn "load failed"
            elif result.Result = null || result.Result.Length = 0 then
                printfn "no result"
            else
                result.Result
                |> Array.iter (fun table ->
                    printfn $"\n{table.TableName.ToUpper()}"
                    
                    // Headers
                    table.Columns
                    |> Seq.cast<DataColumn>
                    |> Seq.iter (fun col -> printf $"{col.ColumnName, -15}")
                    
                    let del = "-" |> String.replicate (15 * table.Columns.Count)
                    printfn $"\n %s{del}"
                    
                    // Rows
                    table.Rows
                    |> Seq.cast<DataRow>
                    |> Seq.iter (fun row ->
                        row.ItemArray
                        |> Array.iter (fun item -> printf $"{item, -15}")
                        printfn ""
                    )
                )
        )
        
        printfn "\nPress any key to continue..."
        Console.ReadKey() |> ignore

open UserInterface
module AnalysisEngine =
    let analyzeFile (plugins: IFlowerSeed list) filePath =
        printfn $"Starting analysis of %s{ FileInfo(filePath).Name }"
        
        plugins
        |> List.map (fun plugin ->
            async {
                try
                    printfn $"Starting plugin: {plugin.Seed}"
                    plugin.Main(filePath) |> ignore // just call it 
                    return (plugin.Seed, plugin.Status)
                with ex ->
                    return (plugin.Seed, FlowerSeedStatus(
                        IsEnabled = false,
                        Result = [||]
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
        printfn $"--- Works {initialState.ActivePlugins.Length}"
        Console.Write("Press any key to continue . . .")
        Console.ReadKey() |> ignore
        
        AnalysisEngine.mainLoop initialState
        0
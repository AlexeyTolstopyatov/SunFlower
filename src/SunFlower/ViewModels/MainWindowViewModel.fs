namespace SunFlower.ViewModels

open System.Net.Mime
open Avalonia
open Avalonia.Styling
open CommunityToolkit.Mvvm.ComponentModel
open CommunityToolkit.Mvvm.Input
open SunFlower.Kernel
open SunFlower.Kernel.Readers
open SunFlower.Services

// CoffeeLake (C) 2026-*
// MIT
// 
// This file contains MainWindow view-model middleware,
// all "movable" properties & command bindings contains here.
// Tasks:
//      -> Converter window opening
//      -> Workspace window redirect
//      -> Deserialize JSON manifest at the startup (async)
//      -> About information window (Flower/Seeds statuslines)
//      ->
type MainWindowViewModel() =
    inherit AvaloniaViewModel()
    [<ObservableProperty>]
    let mutable _files: CorList<FlowerFileInfo> = CorList()
    [<ObservableProperty>]
    let mutable _selectedFile : FlowerFileInfo = Unchecked.defaultof<_>
    
    do
        _files <- JsonService.load "recent"
                  |> List.toArray
                  |> CorList 
        ()
    /// <summary>
    /// File Version of calling assembly
    /// </summary>
    member this.Version: string = "5.0.0.0"
    /// <summary>
    /// Recent files loads at the application startup and
    /// fills the ListBox container
    /// </summary>
    [<ObservableProperty>]
    member this.RecentFiles
        with get() = _files
    [<ObservableProperty>]
    member this.RecentFiles
        with set x = this.SetProperty(ref(_files), x) |> ignore
    
    member this.AddFileCommand =
        let execute _ =
            let newFile = { Name = "new.txt"
                            Size = 1024f
                            Path = failwith "todo"
                            Sign = failwith "todo"
                            Type = failwith "todo" }
            _files.Add(newFile)
            _files
                |> Seq.toList
                |> JsonService.save "recent"
        RelayCommand(execute)
        
    member this.RemoveFileCommand =
        let canExecute _ = _selectedFile <> Unchecked.defaultof<_>
        let execute _ =
            if _selectedFile <> Unchecked.defaultof<_> then
                _files
                    .Remove(_selectedFile)
                    |> ignore
                _files
                    |> Seq.toList
                    |> JsonService.save "recent"
        RelayCommand(execute, canExecute)

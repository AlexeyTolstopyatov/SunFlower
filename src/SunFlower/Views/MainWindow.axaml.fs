namespace SunFlower.Views

open System
open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage
open SunFlower.Kernel.Readers
open SunFlower.Services
open SunFlower.ViewModels

type MainWindow() as this = 
    inherit Window()

    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        // Avalonia hotbar toolkit appears when $Debug
        // configuration is set up  
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
    /// <summary>
    /// Opens another window with loaded Sunflower plugins
    /// and short information about client 
    /// </summary>
    member this.OpenAboutClick(_: obj, _: RoutedEventArgs) =
        // TODO:
        // Move AboutWindow construct into MainWindow->AboutControl
        AboutWindow().Show()
    /// <summary>
    /// Appends selected item(s) into global recent files collection
    /// and updates UI listbox items collection 
    /// </summary>
    member this.ImportAsync() = task {
        // Platform independent logic: Avalonia represents services
        // what uses different platform native functions
        let topLevel = TopLevel.GetTopLevel this
        let options  = FilePickerOpenOptions()
        options.Title <- "Import files"
        options.AllowMultiple <- true
        // Call Avalonia services -> make an OpenFileDialog instance   
        let! storage = topLevel.StorageProvider.OpenFilePickerAsync(options)
        match storage.Count with
        | 0 -> return ()
        | _ ->
        // One or more files defined in storage container
        // Read recent.json async
        let! fileInfos = JsonService.loadAsync<FlowerFileInfo> "recent"
        // Avalonia storage provider contains readonly collection
        // Path of selected item is System.Uri typed.
        // Absolute/Relative paths are coded but here extremely needed raw string of absolute path 
        let newFileInfos = storage
                           |> Seq.map (fun file -> FlowerBinarySeeker.get file.Path.LocalPath)
                           |> Seq.toList
        // Append tail to the deserialized file information list
        do! fileInfos @ newFileInfos
                           |> JsonService.saveAsync "recent"
        let vm = this.DataContext :?> MainWindowViewModel
        do! vm.UpdateRecentContext() // fixme: UpdateRecentContext calls Switches
                                     // instead of correct UI handling
    }
    /// <summary>
    /// Calls Avalonia services to open system GUI opening dialog
    /// </summary>
    member private this.OpenDialog(_: obj, _: RoutedEventArgs) =
        // Call OpenFileDialogAsync with no awaits
        // Looking for Failures
        task {
            try do! this.ImportAsync()
            #if DEBUG
            // Show internal calls + common exception message in the conhost
            with e -> e |> Console.Error.WriteLine
            #else
            // Show simple message in the conhost -> Avalonia can't call
            // to system internals. Open dialog is unavailable -> do nothing
            with e -> "Avalonia can't open dialog" |> Console.Error.WriteLine
            #endif
        } |> Async.AwaitTask |> Async.Start
        ()
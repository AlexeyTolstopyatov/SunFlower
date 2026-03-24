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
    
    member this.OpenAboutClick(_: obj, _: RoutedEventArgs) =
        // TODO:
        // Move AboutWindow construct into MainWindow->AboutControl
        AboutWindow().Show()
    
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
        | 0 -> ()
        | _ ->
        // One or more files defined in storage container
        // Read recent.json async
        let! fileInfos = JsonService.loadAsync<FlowerFileInfo> "recent"
        let newFileInfos = storage
                           |> Seq.map (fun file -> FlowerBinarySeeker.get file.Path.AbsolutePath)
                           |> Seq.toList
        // Append tail to the deserialized file information list
        let updated = fileInfos @ newFileInfos
        do! JsonService.saveAsync "recent" updated
        
        let vm = this.DataContext :?> MainWindowViewModel
        do! vm.UpdateRecentContext() // fixme: UpdateRecentContext calls Switches
                                     // instead of correct UI handling
    }
    
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
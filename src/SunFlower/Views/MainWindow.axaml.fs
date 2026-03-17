namespace SunFlower.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage
open SunFlower.Services

type MainWindow() as this = 
    inherit Window()

    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
    
    member this.OpenAboutClick(_: obj, _: RoutedEventArgs) =
        AboutWindow().Show()
        
    member private this.OpenDialog(s: obj, e: RoutedEventArgs) =
        // Platform-independent logic   
        let topLevel = TopLevel.GetTopLevel(this)
        let options = FilePickerOpenOptions()
        // Importing file/s. Json manifest could append items array
        options.Title <- "Import file"
        options.AllowMultiple <- true
        
        let result = topLevel.StorageProvider.OpenFilePickerAsync(options).Result // lock!
        // If result exists -> saving procedure follows next
        // Call FlowerFileInfo context
        // Write results in "recent.json"
        match result.Count with
        | 0 -> ()
        | _ ->
        // Json service calling. Import target file paths into manifest
        // Refresh control <| VM depends on loaded "recent.json"
        
        ()
namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open SunFlower.Kernel.Readers
open SunFlower.Services

type RecentControl() as this =
    inherit UserControl()
    do this.InitializeComponent()
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
    /// <summary>
    /// Processes the file by given control data 
    /// </summary>
    /// <param name="t"></param>
    member this.Open (t: obj, _: RoutedEventArgs) =
        // Open Workspace sunflower window with given file
        // Despite the fact that given type may be different
        // given file must proceed 
        let file = match t with
                   | :? MenuItem as item -> item.CommandParameter :?> FlowerFileInfo
                   | :? ListBox as list -> list.SelectedItem :?> FlowerFileInfo
                   | ctrl ->
                       #if DEBUG
                       failwith $"\"{ctrl} not bound\""
                       #else
                       failwith "Incomplete match pattern"
                       #endif
    
        let ctx = WorkspaceViewModelFactory.createWorkspace file.Path
        
        WorkspaceWindow(DataContext = ctx).Show()
        ()
    
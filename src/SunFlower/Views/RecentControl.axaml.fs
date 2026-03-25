namespace SunFlower.Views

open System
open Avalonia.Controls
open Avalonia.Input
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open SunFlower.Kernel.Readers


type RecentControl() as this =
    inherit UserControl()
    
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
    member this.Open (listBoxed: obj, _: RoutedEventArgs) =
        // Open Workspace sunflower window with given file
        let listBox = listBoxed :?> ListBox
        
        ()
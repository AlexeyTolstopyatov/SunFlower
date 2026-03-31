namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml


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
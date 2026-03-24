namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open SunFlower.Kernel.Readers


type RecentControl() as this =
    inherit UserControl()
    
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        AvaloniaXamlLoader.Load(this)
        
    member this.Open (file: obj, _: RoutedEventArgs) =
        // Open Workspace sunflower window with given file
        let _fileInfo = file :?> FlowerFileInfo
        
        -1
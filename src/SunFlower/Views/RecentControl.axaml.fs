namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Markup.Xaml


type RecentControl() as this =
    inherit UserControl()
    
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        #if DEBUG
        AvaloniaXamlLoader.Load(this)
        #endif
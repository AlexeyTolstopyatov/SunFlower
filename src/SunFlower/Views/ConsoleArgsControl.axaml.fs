namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Markup.Xaml

type ConsoleArgsControl() as this =
    inherit UserControl()
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        #if DEBUG
        AvaloniaXamlLoader.Load(this)
        #endif

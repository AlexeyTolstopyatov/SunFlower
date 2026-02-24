namespace SunFlower.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Markup.Xaml

[<Class>]
type AboutWindow() as this =
    inherit Window()

    do
        this.InitializeComponent()

    member private this.InitializeComponent() =
        #if DEBUG
        this.AttachDevTools()
        #endif

        AvaloniaXamlLoader.Load(this)

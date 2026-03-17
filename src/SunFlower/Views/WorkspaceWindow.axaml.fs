namespace SunFlower.Views

open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage

type WorkspaceWindow() as this =
    inherit Avalonia.Controls.Window()
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        #if DEBUG
        AvaloniaXamlLoader.Load(this)
        #endif
    
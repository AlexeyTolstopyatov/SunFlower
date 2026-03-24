namespace SunFlower.Views

open Avalonia.Markup.Xaml

type WorkspaceWindow() as this =
    inherit Avalonia.Controls.Window()
    do
        this.InitializeComponent()
        
    member private this.InitializeComponent() =
        #if DEBUG
        // Avalonia hotbar toolkit appears when $Debug
        // configuration is set up  
        // this.AttachDevTools()
        #endif
        AvaloniaXamlLoader.Load(this)
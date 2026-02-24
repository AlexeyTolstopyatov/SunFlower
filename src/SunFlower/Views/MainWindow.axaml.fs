namespace SunFlower.Views

open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml

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
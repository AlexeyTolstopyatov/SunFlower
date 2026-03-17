namespace SunFlower.ViewModels

open Avalonia.Controls
open CommunityToolkit.Mvvm.ComponentModel
// CoffeeLake (C) 2026-*
// MIT
// 
// This file contains MainWindow view-model middleware,
// all "movable" properties & command bindings contains here.
// Tasks:
//      -> Converter window opening
//      -> Workspace window redirect
//      -> Deserialize JSON manifest at the startup (async)
//      -> About information window (Flower/Seeds statuslines)
//      ->
type MainWindowViewModel() as this =
    inherit AvaloniaViewModel()
    [<ObservableProperty>]
    let mutable _currentViewModel: AvaloniaViewModel option = None
    do
        _currentViewModel <- Some(RecentViewModel())
        ()
    member this.Version: string = "5.0.0.0"
    
    [<ObservableProperty>]
    member this.CurrentViewModel
        with get () = _currentViewModel
        and set value =
            _currentViewModel <- value
            this.OnPropertyChanged()

    member this.SwitchRecent() =
        this.CurrentViewModel <- Some(RecentViewModel())
    member this.SwitchOpening() =
        this.CurrentViewModel <- Some(ConsoleArgsViewModel())
    member this.SwitchConverter() =
        this.CurrentViewModel <- Some(ConverterViewModel())
    
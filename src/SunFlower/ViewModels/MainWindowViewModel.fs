namespace SunFlower.ViewModels

open SunFlower.Kernel.Services

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
    member this.Version: string = "5.0.0.0"
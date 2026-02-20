namespace SunFlower.ViewModels

open SunFlower.Models
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
type MainWindowViewModel() =
    inherit ViewModelBase()
    /// <summary>
    /// Version of calling assembly (file version info) 
    /// </summary>
    member this.Version: string = "5.0.0.0"
    /// <summary>
    /// List of loaded recent files from JSON
    /// </summary>
    member this.Recent: List<FileInfoModel> = []
    /// <summary>
    /// Represents JUST STRINGS of placed plugins into
    /// </summary>
    member this.Plugins: List<PluginInfoModel> = []
    
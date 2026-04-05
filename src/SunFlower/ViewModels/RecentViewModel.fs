namespace SunFlower.ViewModels

open System.IO
open Avalonia
open Avalonia.Controls
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower
open SunFlower.Kernel.Readers
open SunFlower.Models
open SunFlower.Services

type RecentViewModel() =
    inherit AvaloniaViewModel()
    let mutable _model = RecentModel()
    
    member this.UpdateListAsync() = task {
        do! _model.LoadRecentAsync()
        this.Recent <- _model.Recent
    }
    /// <summary>
    /// Reference to recent files collection
    /// </summary>
    [<ObservableProperty>]
    member this.Recent
        with get () = _model.Recent
        and set x =
            _model.Recent <- x
            this.OnPropertyChanged()
    /// <summary>
    /// Removes target from recent files collection
    /// Updates view model -> UI
    /// </summary>
    /// <param name="item">record with target item</param>
    member this.RemoveItemAsync(item: FlowerFileInfo) = task {
        match this.Recent.Contains(item) with
        | false -> return ()
        | true ->
        this.Recent.Remove(item)
            |> ignore
        do! this.Recent
            |> Seq.toList
            |> JsonService.saveAsync "recent"
    }
    /// <summary>
    /// Removes target from recent files collection,
    /// target file from filesystem if this path of record is valid
    /// (target file still exists) 
    /// </summary>
    /// <param name="item">record with target item</param>
    member this.RemoveFileAsync(item: FlowerFileInfo) = task {
        match this.Recent.Contains(item) && File.Exists(item.Path) with
        | false -> return ()
        | true ->
        // Make sure that file still exists before deleting it
        // from data sources.
        this.Recent.Remove(item)
            |> ignore
        do! this.Recent
            |> Seq.toList
            |> JsonService.saveAsync "recent"
        
        File.Delete item.Path
    }
    member this.OpenItem(item: FlowerFileInfo) =
        // fixme: Костыли блять заебали уже внатуре
        let viewModel = WorkspaceViewModelFactory.createWorkspace(item.Path)
        let workspace = WindowLocatorFactory.locateWindow(viewModel) :?> Window
        
        workspace.DataContext <- viewModel
        workspace.Show() 
namespace SunFlower.ViewModels

open System
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Kernel.Readers
open SunFlower.Models
open SunFlower.Services

type RecentViewModel() =
    inherit AvaloniaViewModel()
    
    let mutable _model = RecentModel()
    /// Not updates VM async
    member this.UpdateListAsync() = task {
        do! _model.LoadRecentAsync()
        this.Recent <- _model.Recent
    }
    [<ObservableProperty>]
    member this.Recent
        with get () = _model.Recent
        and set x =
            _model.Recent <- x
            this.OnPropertyChanged()
    
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
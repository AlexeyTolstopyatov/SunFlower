namespace SunFlower.ViewModels

open System
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Models

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

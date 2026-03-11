namespace SunFlower.ViewModels

open SunFlower.Models

type RecentViewModel() =
    inherit AvaloniaViewModel()
    
    let mutable _model = RecentModel()
    
    member this.Recent
        with get () = _model.Recent
        and set x = this.SetProperty(ref _model.Recent, x) |> ignore

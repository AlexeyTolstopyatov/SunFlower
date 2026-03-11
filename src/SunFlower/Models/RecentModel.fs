namespace SunFlower.Models

open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Kernel
open SunFlower.Kernel.Readers
open SunFlower.Services

type RecentModel() =
    inherit ObservableObject()
    let mutable _files: CorList<FlowerFileInfo> = CorList()
    
    do
        _files <- JsonService.load "recent"
                  |> List.toArray
                  |> CorList 
        
    [<ObservableProperty>]
    member this.Recent
        with get () = _files
        and set x = this.SetProperty(ref _files, x) |> ignore


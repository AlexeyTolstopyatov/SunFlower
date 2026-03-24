namespace SunFlower.Models

open System.Collections.ObjectModel
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Kernel.Readers
open SunFlower.Services

type RecentModel() =
    inherit ObservableObject()
    let mutable _files: ObservableCollection<FlowerFileInfo> = ObservableCollection()
    do
        // Don't lose context -> run it synchronously
        _files <- JsonService.load "recent"
                  |> seq
                  |> ObservableCollection<FlowerFileInfo>
        
    [<ObservableProperty>]
    member this.Recent
        with get () = _files
        and set x =
            this.SetProperty(ref _files, x) |> ignore
            this.OnPropertyChanged()
        
    member this.LoadRecentAsync() = task {
        let! files = JsonService.loadAsync "recent"
        
        _files.Clear()
        for f in files do _files.Add(f)
    }
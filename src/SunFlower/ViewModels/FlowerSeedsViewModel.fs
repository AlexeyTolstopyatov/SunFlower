namespace SunFlower.ViewModels

open System
open System.IO
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Kernel
open SunFlower.Kernel.Services

[<Class>]
type FlowerSeedsViewModel() =
    inherit AvaloniaViewModel()
    let mutable systemTable: CorList<FlowerVersionInfo> = CorList()
    let mutable aboutText: string = ""
    do
        systemTable <- FlowerCompatibility.getFlowerVersionInfoList()
        aboutText <- File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", "about.txt"))
    
    member this.SystemTable
        with get(): CorList<FlowerVersionInfo> = systemTable
        and set x =
            this.SetProperty(ref systemTable, x)
            |> ignore
    [<ObservableProperty>]    
    member this.AboutText
        with get(): string = aboutText
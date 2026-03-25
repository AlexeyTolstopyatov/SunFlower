namespace SunFlower.ViewModels

open System
open System.IO
open System.Threading.Tasks
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Kernel
open SunFlower.Kernel.Services
open SunFlower.Services

[<Class>]
type AboutViewModel() =
    inherit AvaloniaViewModel()
    let mutable systemTable: CorList<FlowerVersionInfo> = CorList()
    let mutable aboutText: string = ""
    do
        systemTable <- FlowerCompatibility.getFlowerVersionInfoList()
        aboutText <- File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", "about.txt"))
    
    member this.SystemTable
        with get(): CorList<FlowerVersionInfo> = systemTable
        and set x =
            this.SetProperty(ref(systemTable), x)
            |> Console.WriteLine
    [<ObservableProperty>]    
    member this.AboutText
        with get(): string = aboutText
namespace SunFlower.ViewModels

open SunFlower.Models

type MainWindowViewModel() =
    inherit ViewModelBase()

    member this.Version: string = "5.0.0.0"
    member this.Recent: List<FileInfoModel> = []
    
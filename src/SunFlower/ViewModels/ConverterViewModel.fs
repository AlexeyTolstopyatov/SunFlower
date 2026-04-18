namespace SunFlower.ViewModels

open CommunityToolkit.Mvvm.ComponentModel

[<Class>]
type ConverterViewModel() =
    inherit AvaloniaViewModel()
    let mutable _anyTypeText: string = ""

    [<ObservableProperty>]
    member this.AnyTypeText
        with get () = _anyTypeText
        and set x =
            _anyTypeText <- x
            //this.SetProperty(ref _anyTypeText, x) |> ignore
            this.OnPropertyChanged()

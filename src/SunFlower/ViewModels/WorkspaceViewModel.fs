namespace SunFlower.ViewModels

open System.Collections.ObjectModel
open SunFlower.Abstractions
open SunFlower.Kernel.Writers    

type WorkspaceViewModel(sourceList, messages) =
    inherit AvaloniaViewModel()
    let mutable _messages: string array = messages
    let mutable _sourceList: ObservableCollection<IFlowerSeed> = sourceList
    //let mutable _source: string = FlowerMarkdownWriter.write(sourceList[0].Status.Results)
    member this.Source with get() =
        match sourceList.Count with
        | 0 -> "No data"
        | _ ->
            FlowerMarkdownWriter.write _sourceList[0].Status.Results
    member this.SourceList with get() = _sourceList
    member this.Messages with get() = _messages
    // 7 years in .NET and I still know nothing...
    // Constructor must initialize object once in single thread without any asynchronous calls
    // That's why I got undefined behavior here.
    //
    // To Avoid it, current VM constructor must initialize in another thread
    // without any UI calls from there.
    new() =
        WorkspaceViewModel(ObservableCollection(), [||])
        
    
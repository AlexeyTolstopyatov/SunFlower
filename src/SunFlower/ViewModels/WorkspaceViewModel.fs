namespace SunFlower.ViewModels

open System.Collections.ObjectModel
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Abstractions
open SunFlower.Kernel.Writers

type WorkspaceViewModel(sourceList: ObservableCollection<IFlowerSeed>, messages: string array, path: string) as this =
    inherit AvaloniaViewModel()

    let mutable _path = path
    let mutable _messages = messages
    let mutable _sourceList = sourceList
    let mutable _sourceIndex = 0
    let mutable _sourceException = ""
    let mutable _sourceExceptionMessage = ""

    do this.updateFromCurrentSource ()

    [<ObservableProperty>]
    member this.SourceList
        with get () = _sourceList
        and set (value) =
            _sourceList <- value
            this.OnPropertyChanged()
            this.updateFromCurrentSource ()

    member this.SourceIndex
        with get () = _sourceIndex
        and set (value) =
            if _sourceIndex <> value then
                _sourceIndex <- value
                this.OnPropertyChanged()
                this.updateFromCurrentSource ()

    member this.Source =
        if _sourceList.Count = 0 then
            "No data"
        else
            FlowerMarkdownWriter.write _sourceList[_sourceIndex].Status.Results

    member this.SourceExceptionMessage
        with get () = _sourceExceptionMessage
        and set (value) =
            _sourceExceptionMessage <- value
            this.OnPropertyChanged()

    member this.SourceException
        with get () = _sourceException
        and set (value) =
            _sourceException <- value
            this.OnPropertyChanged()

    member this.Messages = _messages
    member this.Path = _path

    /// <summary>
    /// Reacts at the UI changes: updates model by current source index
    ///
    /// SourceIndex bound with the ComboBox Visual element "SelectionIndex"
    /// </summary>
    member this.updateFromCurrentSource() =
        match sourceList.Count with
        | 0 ->
            _sourceExceptionMessage <- "No data source"
            _sourceException <- "No source"
        | _ ->
            let seed = _sourceList[_sourceIndex]

            match seed.Status.LastError with
            | null ->
                _sourceExceptionMessage <- "Working correctly"
                _sourceException <- ""
            | e ->
                _sourceExceptionMessage <- e.Message
                _sourceException <- e.ToString()

        this.OnPropertyChanged(nameof (this.SourceExceptionMessage))
        this.OnPropertyChanged(nameof (this.SourceException))
        this.OnPropertyChanged(nameof (this.Source))

    new() = WorkspaceViewModel(ObservableCollection(), [||], "")

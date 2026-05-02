namespace SunFlower.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open System.Linq
open AvaloniaEdit.Document
open AvaloniaHex.Document
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Abstractions
open SunFlower.Kernel.Services
open SunFlower.Kernel.Writers
open SunFlower.Services

type WorkspaceViewModel(sourceList: ObservableCollection<FlowerSeedData>, path: string) as this =
    inherit AvaloniaViewModel()
    /// <summary>
    /// Given file path. Shows in the title of WorkspaceWindow UI
    /// </summary>
    let mutable _path = path
    /// <summary>
    /// Loaded plugins list. Given by WorkspaceViewModelFactory
    /// </summary>
    let mutable _sourceList = sourceList
    /// <summary>
    /// Selected source index (updates from UI by user) 
    /// </summary>
    let mutable _sourceIndex = 0
    /// <summary>
    /// Selected source Exception::ToString() string
    /// </summary>
    let mutable _sourceException = ""
    /// <summary>
    /// Selected Source Exception::Message string
    /// </summary>
    let mutable _sourceExceptionMessage = ""
    /// <summary>
    /// Selected source kind. (CODE/DATA/PROC etc.) category
    /// </summary>
    let mutable _kind = ""
    /// <summary>
    /// Selected source [FlowerContract] version
    /// </summary>
    let mutable _version = Version()
    /// <summary>
    /// Array of rendered results.
    /// </summary>
    let mutable _results = _sourceList
                               |> Seq.map (fun s -> s.render())
                               |> Seq.toArray

    do this.updateFromCurrentSource ()

    /// <summary>
    /// Loaded plugins list
    /// </summary>
    [<ObservableProperty>]
    member this.SourceList
        with get () = _sourceList
        and set value =
            _sourceList <- value
            this.OnPropertyChanged()
            this.updateFromCurrentSource ()
    
    /// <summary>
    /// Selected data source in the UI BimboBox
    /// </summary>
    member this.SourceIndex
        with get () = _sourceIndex
        and set value =
            if _sourceIndex <> value then
                _sourceIndex <- value
                this.OnPropertyChanged()
                this.updateFromCurrentSource ()
    member this.PreferRender
        with get () = _sourceList[_sourceIndex].kind = SeedTarget.Data
    /// <summary>
    /// Rendered result source by selected index
    /// </summary>
    member this.Source =
        match _sourceList.Count with
        | 0 -> "no data"
        | _ -> _results[_sourceIndex]
    member this.BinaryDocument=
        new MemoryBinaryDocument(File.ReadAllBytes(path));

    member this.SourceDocument =
        match _sourceList.Count with
        | 0 -> TextDocument("; No data source present")
        | _ -> TextDocument(_results[_sourceIndex])
    /// <summary>
    /// Reference of _sourceExceptionMessage
    /// </summary>
    member this.SourceExceptionMessage
        with get () = _sourceExceptionMessage
        and set value =
            _sourceExceptionMessage <- value
            this.OnPropertyChanged()

    /// <summary>
    /// Reference of _sourceExceptioin
    /// </summary>
    member this.SourceException
        with get () = _sourceException
        and set value =
            _sourceException <- value
            this.OnPropertyChanged()

    /// <summary>
    /// Reference of the _version
    /// </summary>
    member this.SourceVersion
        with get () = _version
        and set value =
            _version <- value
            this.OnPropertyChanged()

    /// <summary>
    /// Reference of the _kind
    /// </summary>
    member this.SourceKind
        with get () = _kind
        and set value =
            _kind <- value
            this.OnPropertyChanged()

    member this.Path = _path

    /// <summary>
    /// Reacts at the UI changes: updates model by current source index
    /// SourceIndex bound with the ComboBox Visual element "SelectionIndex"
    /// </summary>
    member this.updateFromCurrentSource() =
        match sourceList.Count with
        | 0 ->
            _sourceExceptionMessage <- "No data source"
            _sourceException <- "No source"
        | _ ->
            let data = _sourceList[_sourceIndex]

            match data.hasError () with
            | false ->
                _sourceExceptionMessage <- "Working correctly"
                _sourceException <- ""
                _kind <- data.kind |> string
                _version <- data.version
            | true ->
                _sourceExceptionMessage <- data.lastError ()
                _sourceException <- data.lastErrorTrace ()
                _kind <- "?"
                _version <- Version()
        
        this.OnPropertyChanged(nameof this.SourceExceptionMessage)
        this.OnPropertyChanged(nameof this.SourceException)
        this.OnPropertyChanged(nameof this.Source)
        this.OnPropertyChanged(nameof this.SourceKind)
        this.OnPropertyChanged(nameof this.SourceVersion)
        this.OnPropertyChanged(nameof this.SourceIndex)
        this.OnPropertyChanged(nameof this.SourceDocument)
        this.OnPropertyChanged(nameof this.PreferRender)
    new() = WorkspaceViewModel(ObservableCollection(), "")

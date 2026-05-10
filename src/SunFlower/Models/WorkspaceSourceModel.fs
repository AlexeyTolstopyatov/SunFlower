namespace SunFlower.Models

open System
open System.Collections.ObjectModel
open System.IO
open System.Linq
open AvaloniaEdit.Document
open AvaloniaHex.Document
open CommunityToolkit.Mvvm.ComponentModel
open SunFlower.Abstractions
open SunFlower.Kernel.Services
open SunFlower.ViewModels

[<Class>]
type WorkspaceSourceModel(sourceList: ObservableCollection<FlowerSeedData>, filePath: string) as this =
    inherit AvaloniaViewModel()
    /// <summary>
    /// Given file path. Shows in the title of WorkspaceWindow UI
    /// </summary>
    let mutable path = filePath
    /// <summary>
    /// Loaded plugins list. Given by WorkspaceViewModelFactory
    /// </summary>
    let mutable sourceList = sourceList
    /// <summary>
    /// Selected source index (updates from UI by user) 
    /// </summary>
    let mutable sourceIndex = 0
    /// <summary>
    /// Selected source Exception::ToString() string
    /// </summary>
    let mutable sourceException = ""
    /// <summary>
    /// Selected Source Exception::Message string
    /// </summary>
    let mutable sourceExceptionMessage = ""
    /// <summary>
    /// Selected source kind. (CODE/DATA/PROC etc.) category
    /// </summary>
    let mutable kind = ""
    /// <summary>
    /// Selected source [FlowerContract] version
    /// </summary>
    let mutable version = Version()
    /// <summary>
    /// Array of rendered results.
    /// </summary>
    let mutable results =
        sourceList
                               |> Seq.map (fun s -> s.render())
                               |> Seq.toArray
    /// <summary>
    /// State flag for View and View Model: Open selected source in the Markdown control
    /// or show it in the TextEdit control. It might be read-only property but
    /// parent model uses this flag too
    /// </summary>
    let mutable openInMarkdownRender = true
    do this.Update ()
    /// <summary>
    /// Loaded plugins list
    /// </summary>
    [<ObservableProperty>]
    member this.LoadedFlowerSeeds
        with get () = sourceList
        and set value =
            sourceList <- value
            this.OnPropertyChanged()
            this.Update ()
    /// <summary>
    /// Selected data source in the UI BimboBox
    /// </summary>
    member this.SelectedFlowerIndex
        with get () = sourceIndex
        and set value =
            if sourceIndex <> value then
                sourceIndex <- value
                this.OnPropertyChanged()
                this.Update ()
    member this.OpenInMarkdownRender
        with get () = openInMarkdownRender
        and set (x) = openInMarkdownRender <- x
    /// <summary>
    /// Rendered result source by selected index
    /// </summary>
    member this.Source
        with get () =
            match sourceList.Count with
            | 0 -> "no data"
            | _ -> results[sourceIndex]
    /// <summary>
    /// Loaded plugin result into TextDocument container
    /// </summary>
    member this.Document =
        match sourceList.Count with
        | 0 -> TextDocument("# No data source present")
        | _ -> TextDocument(results[sourceIndex])
    /// <summary>
    /// Reference of _sourceExceptionMessage
    /// </summary>
    member this.FlowerSeedMessage
        with get () = sourceExceptionMessage
        and set value =
            sourceExceptionMessage <- value
            this.OnPropertyChanged()
    /// <summary>
    /// Reference of _sourceExceptioin
    /// </summary>
    member this.FlowerSeedException
        with get () = sourceException
        and set value =
            sourceException <- value
            this.OnPropertyChanged()
    member this.FlowerSeedEnabled
        with get () =
            not(this.LoadedFlowerSeeds[this.SelectedFlowerIndex].hasError())
    /// <summary>
    /// Reference of the _version
    /// </summary>
    member this.FlowerSeedVersion
        with get () = version
        and set value =
            version <- value
            this.OnPropertyChanged()

    /// <summary>
    /// Reference of the _kind
    /// </summary>
    member this.FlowerSeedKind
        with get () = kind
        and set value =
            kind <- value
            this.OnPropertyChanged()

    member this.Path = path

    /// <summary>
    /// Reacts at the UI changes: updates model by current source index
    /// SourceIndex bound with the ComboBox Visual element "SelectionIndex"
    /// </summary>
    member this.Update() =
        match sourceList.Count with
        | 0 ->
            sourceExceptionMessage <- "No data source"
            sourceException <- "No source"
        | _ ->
            let currentSource = sourceList[sourceIndex]
            match currentSource.hasError () with
            | true ->
                sourceExceptionMessage <- currentSource.lastError ()
                sourceException <- currentSource.lastErrorTrace ()
                kind <- "Undefined"
                version <- Version()
                sourceIndex <- 1
            | false ->
                sourceExceptionMessage <- "Works."
                sourceException <- ""
                kind <- currentSource.kind |> string
                version <- currentSource.version
                openInMarkdownRender <- (currentSource.kind = SeedTarget.Data)
        this.OnPropertyChanged(nameof this.FlowerSeedMessage)
        this.OnPropertyChanged(nameof this.FlowerSeedException)
        this.OnPropertyChanged(nameof this.Source)
        this.OnPropertyChanged(nameof this.FlowerSeedKind)
        this.OnPropertyChanged(nameof this.FlowerSeedVersion)
        this.OnPropertyChanged(nameof this.SelectedFlowerIndex)
        this.OnPropertyChanged(nameof this.Document)
        this.OnPropertyChanged(nameof this.OpenInMarkdownRender)
    
    new() = WorkspaceSourceModel(ObservableCollection(), "")
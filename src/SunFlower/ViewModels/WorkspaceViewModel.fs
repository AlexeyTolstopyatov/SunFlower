namespace SunFlower.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open AvaloniaEdit.Document
open AvaloniaHex.Document
open AvaloniaHex.Editing
open CommunityToolkit.Mvvm.Input
open SunFlower.Kernel.Services
open SunFlower.Models

type WorkspaceViewModel(sourceList: ObservableCollection<FlowerSeedData>, path: string) as this =
    inherit AvaloniaViewModel()
    let mutable source = WorkspaceSourceModel(sourceList, path)
    let mutable dasm = WorkspaceDisassemblerModel()
    let mutable dasmMode = false
    let mutable dasmFragment = TextDocument()
    /// <summary>
    /// Source model represents all fields of source with this I work
    /// All actions and data bound with loaded plugins for given file (by _path) contains in WorkspaceSourceModel 
    /// </summary>
    member this.SourceModel
        with get () = source
        and set s =
            source <- s
            this.OnPropertyChanged()
    /// <summary>
    /// Userland presents some actions with Hex editor, and once of them is an interactive disassembling
    /// of selected fragments in the loaded Binary document. Disassembler data contains/updates here
    ///
    /// Depending on selected model of disassembler from SunFlower.Dasm, workspace updates and
    /// SourceModel.Source 
    /// </summary>
    member this.DasmModel
        with get () = dasm
        and set d =
            dasm <- d
            this.OnPropertyChanged()
    
    member this.Source
        with get () =
            match dasmMode with
            | false -> this.SourceModel.Document
            | true -> dasmFragment
        and set (s: TextDocument) =
            match dasmMode with
            | false -> ()
            | true ->
                this.SourceModel.OpenInMarkdownRender <- false
                dasmFragment <- s
                this.OnPropertyChanged()
    /// <summary>
    /// Loaded Hex view of file by given path
    /// </summary>
    member this.File
        with get () = new MemoryBinaryDocument(File.ReadAllBytes(path))
    
    [<RelayCommand>]
    member this.Command (selection: Selection) =
        selection.Range.ByteLength |> Console.WriteLine    
    /// <summary>
    /// Updates all children in "/View" 
    /// </summary>
    member this.Update() =
        this.SourceModel.Update()
        this.OnPropertyChanged (nameof this.File)
        this.OnPropertyChanged (nameof this.Source)
        
    new() = WorkspaceViewModel(ObservableCollection(), String.Empty)

namespace SunFlower.ViewModels

open System
open System.Collections.ObjectModel
open System.IO
open AvaloniaEdit.Document
open AvaloniaHex.Document
open AvaloniaHex.Editing
open CommunityToolkit.Mvvm.ComponentModel
open CommunityToolkit.Mvvm.Input
open SunFlower.Kernel.Services
open SunFlower.Models

type WorkspaceViewModel(sourceList: ObservableCollection<FlowerSeedData>, path: string) =
    inherit AvaloniaViewModel()
    let mutable source = WorkspaceSourceModel(sourceList, path)
    let mutable dasm = WorkspaceDisassemblerModel()
    let mutable disassemblerModeEnabled = false
    let mutable disassembledFragment = TextDocument()
    
    /// <summary>
    /// Source model represents all fields of source with this I work
    /// All actions and data bound with loaded plugins for given file (by _path) contains in WorkspaceSourceModel
    /// </summary>
    member this.SourceModel
        with get () =
            source
        and set s =
            source <- s
            this.DisassemblerModeEnabled <- false
            this.OnPropertyChanged()
            this.OnPropertyChanged(nameof this.DisplayDocument)

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
            this.OnPropertyChanged(nameof this.DisplayDocument)

    [<ObservableProperty>]
    member this.DisplayDocument =
        if disassemblerModeEnabled then disassembledFragment else this.SourceModel.Document

    member this.DisassemblerModeEnabled
        with get () = disassemblerModeEnabled
        and set d =
            disassemblerModeEnabled <- d
            this.OnPropertyChanged()
            this.OnPropertyChanged(nameof this.DisplayDocument)

    member this.Source
        with set (s: TextDocument) =
            if disassemblerModeEnabled then
                this.SourceModel.OpenInMarkdownRender <- false
                disassembledFragment <- s

            this.OnPropertyChanged(nameof this.DisplayDocument)

    /// <summary>
    /// Loaded Hex view of file by given path
    /// </summary>
    member this.File = new MemoryBinaryDocument(File.ReadAllBytes(path))

    [<RelayCommand>]
    member this.TranslateSelected(selection: Selection) =
        match selection.Range.ByteLength.Equals(0) with
        | true -> ()
        | false ->
            this.DasmModel.SelectedModeIndex <-
                if
                    this.DasmModel.SelectedModeIndex > this.DasmModel.Modes.Length
                    || this.DasmModel.SelectedModeIndex < 0
                then
                    0
                else
                    this.DasmModel.SelectedModeIndex

            let decoder = this.DasmModel.Modes[this.DasmModel.SelectedModeIndex].action
            let bytesRange = Array.zeroCreate(int selection.Range.ByteLength).AsSpan()

            this.File.ReadBytes(selection.Range.Start.ByteIndex, bytesRange)

            let lines =
                try
                    decoder (bytesRange.ToArray())
                with e ->
                    [ "# Disassembler died with following words: "; $"# {e}" ]

            let text = String.Join("\r\n", lines)
            // Update properties -> show results
            disassembledFragment <- TextDocument(text)
            this.DisassemblerModeEnabled <- true
            this.SourceModel.OpenInMarkdownRender <- false
            this.OnPropertyChanged(nameof this.DisplayDocument)

    /// <summary>
    /// Updates all children in "/View"
    /// </summary>
    member this.Update() =
        this.SourceModel.Update()
        
        this.OnPropertyChanged(nameof this.File)
        this.OnPropertyChanged(nameof this.DisplayDocument)

    new() = WorkspaceViewModel(ObservableCollection(), String.Empty)

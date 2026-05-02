namespace SunFlower.Views

open System
open System.IO
open Avalonia
open Avalonia.Controls
open Avalonia.Interactivity
open Avalonia.Markup.Xaml
open Avalonia.Platform.Storage
open AvaloniaEdit
open AvaloniaEdit.TextMate
open SunFlower.ViewModels
open TextMateSharp.Grammars

type WorkspaceWindow() as this =
    inherit Window()
    do this.InitializeComponent()

    member private this.InitializeComponent() =
#if DEBUG
        // Avalonia hotbar toolkit appears when $Debug
        // configuration is set up
        // this.AttachDevTools()
#endif
        AvaloniaXamlLoader.Load(this)
        
    member this.LoadAssemblyGrammar(t: obj, e: RoutedEventArgs) =
        let editor = this.FindControl<TextEditor>("SourceEditor")
        let path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AT&T.JSON-tmLanguage") // was ""
        
        let options = RegistryOptions(ThemeName.KimbieDark)
        let textMateInstallation = editor.InstallTextMate(options)
        textMateInstallation.SetGrammarFile(path)
        ()

    member this.ExportAsync() =
        task {
            // Platform independent logic: Avalonia represents services
            // what uses different platform native functions
            let topLevel = TopLevel.GetTopLevel this
            let options = FilePickerSaveOptions()
            options.Title <- "Export document"
            options.FileTypeChoices <- [| FilePickerFileType("Markdown Document (*.md)", Patterns = [| ".md" |]) |]
            // Call Avalonia services -> make an OpenFileDialog instance
            // and wait dialog closing event (till the OK/Cancel/Close result 's given)
            let! storage = topLevel.StorageProvider.SaveFilePickerAsync(options)

            match storage.Name.Length with
            | 0 -> return ()
            | _ ->
                // The storage & current instance of DataContext still exist
                // nullable-objects check are redundant.
                let ctx = this.DataContext :?> WorkspaceViewModel

                match isNull storage with
                | true -> return ()
                | false ->

                    let! stream = storage.OpenWriteAsync()
                    use writer = new StreamWriter(stream)

                    do! writer.WriteAsync ctx.Source
        }

    member this.SaveDialog(_: obj, _: RoutedEventArgs) =
        // Call SaveFileDialogAsync with no awaits
        // Looking for Failures
        task {
            try
                do! this.ExportAsync()
#if DEBUG
            // Show internal calls + common exception message in the conhost
            with e ->
                e |> Console.Error.WriteLine
#else
            // Show simple message in the conhost -> Avalonia can't call
            // to system internals. Open dialog is unavailable -> do nothing
            with e ->
                "Unable to export" |> Console.Error.WriteLine
#endif
        }
        |> Async.AwaitTask
        |> Async.Start

    member this.SavePdfDialog(_: obj, _: RoutedEventArgs) =
        let ctx = this.DataContext :?> WorkspaceViewModel

        ()

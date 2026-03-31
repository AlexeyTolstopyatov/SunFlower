namespace SunFlower

open System
open System.IO
open System.Threading.Tasks
open Avalonia
open Avalonia.Controls.ApplicationLifetimes
open Avalonia.Markup.Xaml
open SunFlower.Services
open SunFlower.ViewModels
open SunFlower.Views

type App() =
    inherit Application()

    override this.Initialize() =
            AvaloniaXamlLoader.Load(this)

    override this.OnFrameworkInitializationCompleted() =
        match this.ApplicationLifetime with
        | :? IClassicDesktopStyleApplicationLifetime as desktop ->
             match desktop.Args.Length <> 1 with
             | true -> desktop.MainWindow <- MainWindow(DataContext = MainWindowViewModel())
             | false ->
             // Check if given path is invalid -> throw message into stderr & force exit
             // Supported only 1 given file window instances will be only one
             let target = desktop.Args[0]
             match File.Exists target with
             | true ->
                 desktop.MainWindow <- WorkspaceWindow(DataContext = WorkspaceViewModelFactory.createWorkspace(target))
             | false ->
                 $"Not found \"{target}\". Aborted" |> Console.Error.WriteLine
                 Environment.Exit -1
             ()
        | _ -> ()

        base.OnFrameworkInitializationCompleted()
    
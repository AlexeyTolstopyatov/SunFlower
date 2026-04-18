module SunFlower.Services.WorkspaceViewModelFactory

open System.Collections.ObjectModel
open SunFlower.Kernel.Services
open SunFlower.ViewModels

let createWorkspace (path: string) =
    let manager = FluentFlowerManager()
    // Kernel loads all available results and
    // unloads all plugin instances with no results (result.Count() = 0)
    // All unloaded plugins have reasons which described in kernel "Messages"
    let seeds =
        manager
            .loadAllFlowerSeeds()
            .updateAllInvokedFlowerSeeds(path)
            //.unloadUnusedFlowerSeeds()
            .Seeds
        |> ObservableCollection

    WorkspaceViewModel(seeds, manager.Messages.ToArray(), path)

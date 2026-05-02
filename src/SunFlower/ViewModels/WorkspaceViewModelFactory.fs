module SunFlower.Services.WorkspaceViewModelFactory

open System.Collections.ObjectModel
open SunFlower.Kernel.Services
open SunFlower.ViewModels

let createWorkspace (path: string) =
    // Kernel loads all available results and
    // unloads all plugin instances with no results (result.Count() = 0)
    // All unloaded plugins have reasons which described in kernel "Messages"
    // let data = FlowerManager.touch path
    //            |> FlowerManager.activateAll
    //            |> FlowerManager.updateAll
    //            |> FlowerManager.collect
    //            |> ObservableCollection
    //
    let data = FluentFlowerManager.createInstance()
                                  .activateAll()
                                  .updateAll(path)
                                  .Seeds
                                  |> ObservableCollection
    
    WorkspaceViewModel(data, path)

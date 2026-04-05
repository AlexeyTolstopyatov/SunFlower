module SunFlower.WindowLocatorFactory

open System
open Avalonia.Controls
open SunFlower.ViewModels

/// <summary>
/// Tries to find view .NET metadata by given view model
/// and activates it in success. 
/// </summary>
/// <param name="data">
/// View model name must be the same with View control name
/// </param>
let locateWindow (data: obj): Control =
    match isNull data with
    | true -> null
    | false ->
    // If "...ViewModel" entity found -> try to find Window with the same name
    let name = data.GetType().Name.Replace("ViewModel", "Window", StringComparison.Ordinal)
    let typ = Type.GetType(name)
    
    match isNull typ with
    | true -> failwithf $"\"%s{name}\" was null?" //upcast TextBlock(Text = $"Not found: %s{name}")
    | false -> downcast Activator.CreateInstance(typ)
/// <summary>
/// Compares .NET metadata of given object with AvaloniaViewModel
/// </summary>
/// <param name="data">
/// If casting success, given data is a supported ViewModel class
/// </param>
let isViewModel (data: obj) = data :? AvaloniaViewModel

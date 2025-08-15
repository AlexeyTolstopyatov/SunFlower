namespace SunFlower.Abstractions

open System
open System.Collections.Generic
open System.Runtime.InteropServices.JavaScript
open SunFlower.Abstractions.Types
///
/// CoffeeLake 2025
/// this code licensed under (see GitHub repo) license
///
/// @creator: atolstopyatov2017@vk.com
///

/// <summary>
/// Status entity. All operations result
/// contains here in <see cref="DataTable"/> array.
///
/// Special flags i.e. <c>IsEnabled</c> switch
/// in plugin's body for next usage (or not).
/// </summary>
type FlowerSeedStatus() as status = class
    inherit MarshalByRefObject()
    let mutable isEnabled : Boolean = false
    let mutable lastError : Exception = null
    /// <summary>
    /// Holds on state of seed's usage.
    /// Seeds HashSet contains all plugins (enabled and don't)
    /// but you can disable anybody "by hand"
    /// </summary>
    member public f.IsEnabled
        with get () = isEnabled
        and set flag = isEnabled <- flag
    /// <summary>
    /// Results list of external DLL
    /// </summary>
    member val public Results : List<FlowerSeedResult> = List<FlowerSeedResult>()
    /// <summary>
    /// If result table is null or
    /// Count of results is zero -> connection failed
    /// returns false. 
    /// </summary>
    member val public IsResultExists : Boolean = status.Results.Count <> 0
    /// <summary>
    /// Stores last exception or exceptions chain
    /// </summary>
    member public f.LastError
        with get () = lastError
        and set error = lastError <- error
    end


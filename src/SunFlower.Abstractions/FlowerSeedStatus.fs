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
type FlowerSeedStatus() = class
    inherit MarshalByRefObject()
    let mutable isEnabled : Boolean = false
    let mutable lastError : Exception = null
    let mutable results = List<FlowerSeedResult>()
    let mutable resultsExists : Boolean = results.Count <> 0
    /// <summary>
    /// Holds on state of seed's usage.
    /// Seeds HashSet contains all plugins (enabled and don't)
    /// but you can disable anybody "by hand"
    /// </summary>
    member public _.IsEnabled
        with get () = isEnabled
        and set flag = isEnabled <- flag
    /// <summary>
    /// Results list of external DLL
    /// </summary>
    member public _.Results
        with get () = results
        and set r = results <- r
    /// <summary>
    /// If result table is null or
    /// Count of results is zero -> connection failed
    /// returns false. 
    /// </summary>
    member public _.IsResultExists
        with get () = resultsExists
    /// <summary>
    /// Stores last exception or exceptions chain
    /// </summary>
    member public f.LastError
        with get () = lastError
        and set error = lastError <- error
    end


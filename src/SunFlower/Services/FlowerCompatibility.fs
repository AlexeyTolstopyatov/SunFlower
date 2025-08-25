namespace SunFlower.Services

open System
open System.Data
open System.Reflection
open SunFlower.Abstractions

//
// CoffeeLake (C) 2024-2025
// This part of code licensed under MIT
// Check repo documentation
//
// @creator atolstopyatov2017@vk.com
//
module FlowerCompatibility =
    /// <summary>
    /// Returns Some result or None depends on attribute existence
    /// </summary>
    /// <param name="t"></param>
    let private tryGetFlowerContract (t: Type) : FlowerSeedContractAttribute option =
        let contract = typeof<FlowerSeedContractAttribute>
        t.GetCustomAttributes(contract, false)
        |> Array.tryHead
        |> Option.map (fun attr -> (attr :?> FlowerSeedContractAttribute))
    
    let private addColumn (table: DataTable, str: string) =
        table.Columns.Add(str) |> ignore
        table
    /// <summary>
    /// Calls manager insights for checking compatibility
    /// between contracts of sunflower seed (external DLL)
    /// and manager
    /// </summary>
    /// <param name="path"></param>
    [<CompiledName "Get">]
    let get (path: string) : DataTable =
        let table : DataTable = new DataTable("Compatibility table")
        table.Columns.Add "Version" |> ignore
        table.Columns.Add "Title" |> ignore
        table.Columns.Add "Type" |> ignore
        
        
        
        table
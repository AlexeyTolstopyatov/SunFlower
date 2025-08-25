namespace SunFlower.Services

open System
open System.Data
open System.IO
open System.Reflection
open Microsoft.FSharp.Core
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
    let private tryGetFlowerContract (t: Type) : FlowerSeedContractAttribute option =
        t.GetCustomAttributes(typeof<FlowerSeedContractAttribute>, false)
        |> Array.tryHead
        |> Option.map (fun attr -> attr :?> FlowerSeedContractAttribute)
    
    /// <summary>
    /// Creates a compatibility table with standard columns
    /// </summary>
    let private createCompatibilityTable title =
        let table = new DataTable(title)
        table.Columns.Add("Version", typeof<string>) |> ignore
        table.Columns.Add("Type", typeof<string>) |> ignore
        table.Columns.Add("Is Compatible?", typeof<bool>) |> ignore
        table
    
    /// <summary>
    /// Gets manager version for comparison
    /// </summary>
    let private getManagerVersion () =
        let attr = typeof<FlowerSeedManager>.GetCustomAttribute<FlowerSeedContractAttribute>()
        (attr.MajorVersion, attr.MinorVersion, attr.BuildVersion)
    
    /// <summary>
    /// Checks if a plugin version is compatible with manager
    /// </summary>
    let private isCompatible (pluginAttr: FlowerSeedContractAttribute) (managerMajor, managerMinor, _) =
        // Basic compatibility check: same major version
        pluginAttr.MajorVersion = managerMajor
    
    /// <summary>
    /// Processes a single assembly and adds its types to the compatibility table
    /// </summary>
    let private processAssembly (table: DataTable) (managerVersion: int * int * int) assemblyPath =
        try
            let assembly = Assembly.LoadFrom(assemblyPath)
            assembly.GetTypes()
            |> Array.filter (fun t -> 
                typeof<IFlowerSeed>.IsAssignableFrom(t) && 
                t.IsClass && 
                not t.IsAbstract)
            |> Array.iter (fun t ->
                match tryGetFlowerContract t with
                | Some attr ->
                    let versionStr = $"{attr.MajorVersion}.{attr.MinorVersion}.{attr.BuildVersion}"
                    let compatible = isCompatible attr managerVersion
                    table.Rows.Add(versionStr, t.Name, compatible) |> ignore
                | None ->
                    table.Rows.Add("Contract?!", t.Name, false) |> ignore)
        with
        | ex ->
            table.Rows.Add("Load error", Path.GetFileName(assemblyPath), false) |> ignore
            table.Rows.Add("Error details", ex.Message, false) |> ignore
    
    /// <summary>
    /// Checks compatibility for a single plugin
    /// </summary>
    [<CompiledName "Get">]
    let get (path: string) : DataTable =
        let table = createCompatibilityTable "Plugin Compatibility"
        let managerVersion = getManagerVersion()
        
        // Add manager info
        let (major, minor, build) = managerVersion
        table.Rows.Add($"{major}.{minor}.{build}", "FlowerSeedManager", true) |> ignore
        table.Rows.Add("", "", true) |> ignore  // Empty separator row
        
        // Process the specified assembly
        processAssembly table managerVersion path
        table
    
    /// <summary>
    /// Checks compatibility for all plugins in the Plugins directory
    /// </summary>
    [<CompiledName "GetForAll">]
    let getForAll () : DataTable =
        let table = createCompatibilityTable "All Plugins Compatibility"
        let managerVersion = getManagerVersion()
        
        // Add manager info
        let (major, minor, build) = managerVersion
        table.Rows.Add($"{major}.{minor}.{build}", "FlowerSeedManager", true) |> ignore
        table.Rows.Add("", "", true) |> ignore  // Empty separator row
        
        // Process all assemblies in Plugins directory
        let pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")
        
           
        match Directory.Exists pluginsPath with
        | true -> Directory.GetFiles(pluginsPath, "*.dll")
                            |> Array.iter (processAssembly table managerVersion)
        | false ->
            table.Rows.Add("Directory not found", pluginsPath, false) |> ignore    
        
        table
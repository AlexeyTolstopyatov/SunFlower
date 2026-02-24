namespace SunFlower.Kernel.Services

open System
open System.Data
open System.IO
open System.Reflection
open Microsoft.FSharp.Core
open SunFlower.Kernel
open SunFlower.Abstractions
open System.Diagnostics
//
// CoffeeLake (C) 2024-2026
// This part of code licensed under MIT
// Check repo documentation
//
// @creator atolstopyatov2017@vk.com
//
/// <summary>
/// Compatibility model contains summary about
/// calling assembly & nested activated instances of it
///
/// Uses by other toolkit to hold metadata of 
/// external resources (connected plugins)
/// </summary>
type FlowerVersionInfo = {
    Name: string
    Version: Version
    Contracts: CorList<string * Version>
}
/// <summary>
/// Compatibility module 
/// </summary>
module FlowerCompatibility =
    /// <summary>
    /// Returns Some result or None depends on attribute existence
    /// </summary>
    let private tryGetContract (t: Type) : FlowerSeedContractAttribute option =
        t.GetCustomAttributes(typeof<FlowerSeedContractAttribute>, false)
        |> Array.tryHead
        |> Option.map (fun attr -> attr :?> FlowerSeedContractAttribute)
    
    /// <summary>
    /// Creates a compatibility table with standard columns
    /// </summary>
    let private createCompatTitle title =
        let table = new DataTable(title)
        table.Columns.Add("Version", typeof<string>) |> ignore
        table.Columns.Add("Type", typeof<string>) |> ignore
        table.Columns.Add("Is Compatible?", typeof<bool>) |> ignore
        table
    
    /// <summary>
    /// Gets manager version for comparison
    /// </summary>
    let private getKernelContract () =
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
    let private putInDataTable (table: DataTable) (managerVersion: int * int * int) assemblyPath =
        try
            let assembly = Assembly.LoadFrom(assemblyPath)
            assembly.GetTypes()
            |> Array.filter (fun t -> 
                typeof<IFlowerSeed>.IsAssignableFrom(t) && 
                t.IsClass && 
                not t.IsAbstract)
            |> Array.iter (fun t ->
                match tryGetContract t with
                | Some attr ->
                    let versionStr = $"{attr.MajorVersion}.{attr.MinorVersion}.{attr.BuildVersion}"
                    let compatible = isCompatible attr managerVersion
                    table.Rows.Add(versionStr, t.Name, compatible) |> ignore
                | None ->
                    table.Rows.Add("where?!", t.Name, false) |> ignore)
        with
        | ex ->
            table.Rows.Add("Load error", Path.GetFileName(assemblyPath), false) |> ignore
            table.Rows.Add("Error details", ex.Message, false) |> ignore
    
    let tryGetFlowerVersionInfo(path: string): FlowerVersionInfo =
        // Collect FS & COR metadata .NET object by target path
        // System.Collections.Generic.List<'T> => CorList
        // Resulting instance example: (what I want to see)
        //      return: {
        //          "name": "Exe286.dll",
        //          "version": "1.3.0.0",
        //          "contracts": [
        //              {"name": "80286 CASM", // <-- Common Assembler  
        //               "version": {"major": 3, "minor": 1, "build": 0, "private": 0}},
        //              {"name": "exe286",
        //              "version": {...}}
        //          ]
        //      }
        let contracts: CorList<string * Version> = CorList()
        let fileVersion = FileVersionInfo.GetVersionInfo(path)
        let name = Path.GetFileName(path)
        let version = Version(
            fileVersion.FileMajorPart, 
            fileVersion.FileMinorPart, 
            fileVersion.FileBuildPart, 
            fileVersion.FilePrivatePart
        )
        
        try
            let assembly = Assembly.LoadFrom(path)
            assembly.GetTypes()
            |> Array.filter (fun t -> 
                typeof<IFlowerSeed>.IsAssignableFrom(t) && 
                t.IsClass && 
                not t.IsAbstract)
            |> Array.iter (fun t ->
                    match tryGetContract t with
                    | Some attr ->
                        contracts.Add(t.Name, Version(attr.MajorVersion, attr.MinorVersion, attr.BuildVersion))
                    | None ->
                        contracts.Add("<missing>", Version(0, 0, 0, 0))
            )
        with
        | ex ->
            contracts.Add("<loaderr>", Version(0, 0, 0, 0))
            Console.WriteLine($"{ex}")
            ()
        {
            Name = name
            Version = version
            Contracts = contracts
        }
    /// <summary>
    /// Checks compatibility for a single plugin
    /// </summary>
    [<CompiledName "Get">]
    let get (path: string) : DataTable =
        let table = createCompatTitle "Plugin Compatibility"
        let managerVersion = getKernelContract()
        
        // Add manager info
        let (major, minor, build) = managerVersion
        table.Rows.Add($"{major}.{minor}.{build}", "Sunflower Kernel", true) |> ignore
        table.Rows.Add("", "", true) |> ignore
        
        // Process the specified assembly
        putInDataTable table managerVersion path
        table
    
    /// <summary>
    /// Checks compatibility for all plugins in the Plugins directory
    /// </summary>
    [<CompiledName "GetForAll">]
    let getForAll () : DataTable =
        let table = createCompatTitle "All Plugins Compatibility"
        let managerVersion = getKernelContract()
        
        // Add manager info
        let (major, minor, build) = managerVersion
        table.Rows.Add($"{major}.{minor}.{build}", "SunFlower Kernel", true) |> ignore
        //table.Rows.Add("", "", true) |> ignore  // Empty separator row
        
        // Process all assemblies in Plugins directory
        let pluginsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")
           
        match Directory.Exists pluginsPath with
        | true ->  Directory.GetFiles(pluginsPath, "*.dll") |> Array.iter (putInDataTable table managerVersion)
        | false -> table.Rows.Add("Directory not found", pluginsPath, false) |> ignore    
        
        table
    /// <summary>
    /// See VERSIONING.md for normal explain
    /// This function will repeat information more compressed
    /// for a target. 
    /// </summary>
    /// <param name="path"></param>
    [<CompiledName "GetVerbose">]
    let getAndExplain (path: string) : CorList<string> =
        let strings = CorList<string>()
        let m_maj, m_min, m_bld = getKernelContract()
        
        try
            let assembly = Assembly.LoadFrom(path)
            assembly.GetTypes()
            |> Array.filter (fun t -> 
                typeof<IFlowerSeed>.IsAssignableFrom(t) && 
                t.IsClass && 
                not t.IsAbstract)
            |> Array.iter (fun t ->
                match tryGetContract t with
                | Some attr ->
                    let ver_str = $"{attr.MajorVersion}.{attr.MinorVersion}.{attr.BuildVersion}"
                    strings.Add $"*** {t.Name} v{ver_str} ***"
                    if attr.MajorVersion <> m_maj then
                        strings.Add $" -> Differs with abstractions v{m_maj}.{m_min}.{m_bld}! System must unload it!"
                    else
                        strings.Add $"    Not conflicts with abstractions v{m_maj}.{m_min}.{m_bld}! System can load it."
                    if attr.MinorVersion <> m_min then
                        strings.Add $" -> Differs with minor version. Make sure, it not conflicts with your plugins. System can load it."
                | None ->
                    strings.Add $"*** {t.Name} ***"
                    strings.Add($" -> Doesn't have a [FlowerContract] metadata!"))
        with
        | ex ->
            strings.Add($"Load error: {Path.GetFileName(path)}")
            strings.Add($"\tDetails: {ex.Message}")
        
        
        strings

    [<CompiledName "GetForAllList">]
    let getFlowerVersionInfoList(): CorList<FlowerVersionInfo> = 
        // Collect metadata about Abstractions, Kernel, 
        // nested plugins into /Plugins directory.
        let list = CorList<FlowerVersionInfo>()
        let (major, minor, build) = getKernelContract()
        let kernelVersionInfo = FileVersionInfo
                                    .GetVersionInfo(AppDomain.CurrentDomain.BaseDirectory + "SunFlower.Kernel.dll")

        let contracts = CorList<string * Version>()
        // Kernel .NET assembly adds manually now:
        // .rsrc PE32+ file version and FS name adds to the list firstly
        contracts.Add("Kernel", Version(major, minor, build))
        list.Add({
            Name = Path.GetFileName(kernelVersionInfo.FileName)
            Version = Version(
                kernelVersionInfo.FileMajorPart,
                kernelVersionInfo.FileMinorPart,
                kernelVersionInfo.FileBuildPart,
                kernelVersionInfo.FilePrivatePart
            )
            Contracts = contracts
        })
        // define the /Plugins directory & iterate nested FS objects
        // TryGetFlowerVersionInfo(string) will return FlowerVersionInfo instances anyway
        // 
        // Bad way: returned FlowerVersionInfo instance with <loaderr> name & 0.0.0.0 object version
        // Exceptions chain outputs into Console/configured sh session (stdout)
        let root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")
        match Directory.Exists root with
        | true -> Directory.GetFiles(root, "*.dll") 
                  |> Array.iter (fun i -> list.Add(tryGetFlowerVersionInfo(i)))
        | false -> ()
        // Return not-null list anyway. Nullable objects denied here
        list

        

        
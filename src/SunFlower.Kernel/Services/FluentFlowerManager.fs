namespace SunFlower.Kernel.Services

open System
open System.Collections.Generic
open System.IO
open System.Reflection
open SunFlower.Kernel
open SunFlower.Abstractions
open Microsoft.FSharp.Collections
//
// CoffeeLake 2025-*
// This code licensed under MIT. Please see GitHub repo documentation.
// @creator: atolstopyatov2017@vk.com
//
///
/// SunFlower plugins manager with Fluent API for C#/VB.net client side
/// 
[<FlowerSeedContract(5, 0, 0)>]
type FluentFlowerManager() =
    let mutable seeds: List<FlowerSeedData> = []
    let mutable messages: CorList<string> = CorList<string>()
    /// <summary>
    /// Writes message to Kernel messages storage (CorList of strings)
    ///
    /// Client can read this storage and make
    /// a verbose output or current user.
    ///
    /// </summary>
    /// <param name="str"></param>
    let save (str: string) : unit = messages.Add str
    
    let fromParentMetadata () =
        let parent = typeof<FluentFlowerManager>
        let version = parent.GetCustomAttribute<FlowerSeedContractAttribute> ()
        
        Version (version.MajorVersion, version.MinorVersion, version.BuildVersion)
    
    let mutable parentVersion = Version()
    do parentVersion <- fromParentMetadata ()
    // interface IFlowerSeedManager with
    /// <summary>
    /// Executes all seeds and returns status table
    /// for every seed. Throws child exception chain!!!
    ///
    /// {
    ///     "Title" : "Main actual result"
    ///     "PE32/+ plugin" : "0"
    ///     "CLR plugin" : "-1"
    /// }
    /// </summary>
    [<CompiledName "GetAll">]
    member public this.getAll(path) =
        seeds |> Seq.map (fun x -> KeyValuePair(x.seed.Seed, x.seed.Main path)) |> Dictionary

    [<CompiledName "GetContract">]
    member public this.getContract() = parentVersion |> string

    /// <summary>
    /// Loads sunflower plugins from filesystem
    /// (needed directory: .../Plugins)
    /// </summary>
    [<CompiledName "ActivateAll">]
    member public this.activateAll() =
        let dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")
        let parentType = typeof<IFlowerSeed>

        seeds <-
            Directory.GetFiles(dllPath, "*.dll")
            |> Seq.collect (fun file ->
                try
                    let assembly = Assembly.LoadFrom(file)
                    assembly.GetTypes()
                with _ ->
                    [||])
            |> Seq.filter (fun t -> t.IsClass && not t.IsAbstract && t.IsAssignableTo(parentType))
            |> Seq.choose (fun t ->
                let attr = t.GetCustomAttribute<FlowerSeedContractAttribute>() // .ctor????

                try
                    if attr.MajorVersion = parentVersion.Major then
                        try
                            if attr.MinorVersion <> parentVersion.Minor then
                                $"[!]: {t.Name}#{attr.MajorVersion}.{attr.MinorVersion}.{attr.BuildVersion} differs with {parentVersion.Minor}.{parentVersion.Minor}.{parentVersion.Revision}"
                                |> Console.Error.WriteLine
                            let target = t.GetCustomAttribute<FlowerAttribute>()
                            let seed = Activator.CreateInstance(t) :?> IFlowerSeed
                            Some { seed = seed
                                   kind = target.Target
                                   version = Version(attr.MajorVersion, attr.MinorVersion, attr.BuildVersion) }
                        with e ->
                            e.Message |> Console.Error.WriteLine
                            None
                    else
                        None
                with stop ->
                    $"\r\n >> {t.Name} thrown an error {stop} " |> Console.Error.WriteLine
                    None)
            |> Seq.toList

        this

    /// <summary>
    /// Pointer to storage of all loaded
    /// and initialized (activated)
    /// sunflower plugins interfaces
    /// </summary>
    member public this.Seeds = List seeds
    member public this.Messages = List messages

    [<CompiledName "UnloadUnused">]
    member public this.unloadUnused() =
        seeds <- seeds |> List.where _.seed.Status.IsResultExists |> List.distinct |> Seq.toList
        this

    /// <summary>
    /// Updates <see cref="Seeds"/> collection
    /// by targeting file
    /// </summary>
    /// <param name="path">targeting file</param>
    [<CompiledName "UpdateAll">]
    member public this.updateAll(path) =
        try
            seeds |> Seq.toList |> List.iter (fun x -> x.seed.Main path |> ignore)
        with kernel ->
            $"::STOP\r\n >> {kernel |> string}" |> Console.Error.WriteLine

        this

    /// <summary>
    /// F# makes more strongly inherit process
    /// than C#. This is a Seeds { set; } property
    /// because Seeds { get; +set; } not satisfied
    /// with IFlowerSeedManager rules.
    /// </summary>
    member private this.SeedsInit
        with set s = seeds <- s

    /// <summary>
    /// Makes temporary instance for FlowerSeedManager
    /// </summary>
    [<CompiledName "CreateInstance">]
    static member public createInstance() : FluentFlowerManager = FluentFlowerManager()

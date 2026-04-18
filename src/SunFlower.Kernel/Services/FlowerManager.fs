//
// CoffeeLake (C) 2026-*
//
// This module represents: F# loader
// @creator atolstopyatov2017@vk.com
//
namespace SunFlower.Services

open System
open System.Diagnostics
open System.IO
open System.Reflection
open SunFlower.Abstractions

/// <summary>
/// IFlowerSeed metadata container.
/// </summary>
type FlowerSeedData =
    { seed: IFlowerSeed
      kind: SeedTarget
      version: Version }

/// <summary>
/// State of FlowerManager machine
/// </summary>
type FlowerState =
    { FilePath: string
      SeedsPath: string
      Seeds: seq<FlowerSeedData> }

/// <summary>
/// Loader and plugin manager of SunFlower kernel
/// </summary>
[<FlowerSeedContract(5, 0, 0)>]
module FlowerManager =
    /// <summary>
    /// SunFlower.Kernel file version.
    /// </summary>
    let private loaderMajor =
        FileVersionInfo
            .GetVersionInfo(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SunFlower.Kernel.dll"))
            .FileMajorPart

    /// <summary>
    /// State of manager
    /// </summary>
    let private instance =
        { FilePath = ""
          SeedsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"\Plugins")
          Seeds = [] }

    /// <summary>
    /// Tell manually where the plugins are store
    /// If given path wrong -> uses default path
    /// </summary>
    /// <param name="path">Existing</param>
    /// <param name="isChanged"></param>
    [<CompiledName "TrySetLocation">]
    let trySetLocation (path: string) (isChanged: bool outref) =
        match Directory.Exists path with
        | true ->
            isChanged <- true
            { instance with SeedsPath = path }
        | false ->
            isChanged <- false
            instance

    /// <summary>
    /// Permanently set plugins location
    /// </summary>
    /// <param name="path"></param>
    [<CompiledName "SetLocation">]
    let setLocation (path: string) = { instance with SeedsPath = path }
    /// <summary>
    /// Activate plugin and put it in the loaded plugins collection
    /// </summary>
    /// <param name="path">Target .NET Assembly location</param>
    /// <param name="isOk">True if plugin loaded successfully</param>
    [<CompiledName "Activate">]
    let activate (path: string) (isOk: bool outref) =
        isOk <- false
        
        /// Get types or an empty list if load failed
        let tryGetTypes =
            try
                Assembly.LoadFile(path).GetTypes()
            with | _ -> [||]
        
        let isAssignable (t: Type) =
            let flowerType = typeof<IFlowerSeed>
            t.IsAssignableTo flowerType && not t.IsAbstract && t.IsClass
        
        let havingContext (t: Type) =
            let flowerContract = t.GetCustomAttribute<FlowerSeedContractAttribute>()
            let flowerTarget = t.GetCustomAttribute<FlowerAttribute>()
            try
                
            with
            | e ->
                // Send message to the sky
                e.Message |> Console.Error.WriteLine
                None
            ()
            
        isOk <- true // <-- loaded correctly 
        instance
    /// <summary>
    /// Try to load plugins from <c>SeedsPath</c> directory
    /// </summary>
    [<CompiledName "ActivateAll">]
    let activateAll () =
        /// Returns collection of assembly types or an empty list at all
        /// if file location is invalid
        let tryGetTypes (file: string) =
            try
                Assembly.LoadFile(file).GetTypes()
            with _ ->
                [||]

        /// Returns filter criteria of correct type.
        /// Type must be a derivative of IFlowerSeed interface
        let byAssignableTypes (t: Type) =
            let flowerType = typeof<IFlowerSeed>
            t.IsAssignableTo flowerType && t.IsClass && not t.IsAbstract

        /// Collects optional values from total given types
        /// Watches on the subscribed attributes [Flower] and [FlowerContract]
        let withCorrectMetadata (t: Type) =
            let flowerContract = t.GetCustomAttribute<FlowerSeedContractAttribute>()
            let flower = t.GetCustomAttribute<FlowerAttribute>()
            // Nested metadata are copies to the FlowerSeedData instance
            // And might be used by other modules later.
            // For example:
            // Client looks at the FlowerSeedData::kind field to organize output
            try
                match flowerContract.MajorVersion = loaderMajor with
                | true ->
                    Some
                        { seed = t |> Activator.CreateInstance :?> IFlowerSeed
                          kind = flower.Target
                          version =
                            Version(
                                flowerContract.MajorVersion,
                                flowerContract.MinorVersion,
                                flowerContract.BuildVersion,
                                0
                            ) }
                | false -> None //
            with _ ->
                None
        // Iterate over the loaded assembly types
        // extract necessary payload
        let loadedList =
            Directory.EnumerateFiles(instance.SeedsPath, "*.dll")
            |> Seq.collect tryGetTypes
            |> Seq.filter byAssignableTypes
            |> Seq.choose withCorrectMetadata

        { instance with Seeds = loadedList }

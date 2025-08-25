namespace SunFlower.Services

open System
open System.Collections.Generic
open System.IO
open System.Reflection
open System.Xml.Schema
open SunFlower
open SunFlower.Abstractions
open Microsoft.FSharp.Collections

//
// CoffeeLake 2024-2025
// This code licensed under MIT. Please see GitHub repo documentation
//
// @creator: atolstopyatov2017@vk.com
//
[<FlowerSeedContract (MajorVersion = 2, MinorVersion = 0, BuildVersion = 0)>]
type FlowerSeedManager() =
    let mutable seeds : List<IFlowerSeed> = []
    let mutable messages : CorList<string> = CorList<string>()
    
    /// <summary>
    /// Writes message to Kernel messages storage (CorList of strings)
    ///
    /// Client can read this storage and make
    /// a verbose output or current user.
    /// 
    /// </summary>
    /// <param name="str"></param>
    let save (str: string) : unit =
        messages.Add str
    
    let mutable majorVersion : Int32 = 2
    let mutable minorVersion : Int32 = 0
    let mutable buildVersion : Int32 = 0
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
    member public this.GetAllInvokedFlowerSeeds(path) =
        // c# original ABI
        //
        // var results = new Dictionary<string, int>();
        // foreach (var plugin in Seeds)
        // {
        //     try
        //     {
        //         // Main invoke
        //         var result = plugin.Main(targetingFile);
        //         results.Add(plugin.Seed, result);
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.WriteLine($"Plugin {plugin.Seed} failed: {ex.Message} \n\n {plugin} \n");
        //     }
        // }
        
        // rewritten to F# 
        $"Sunflower Kernel::GetAllInvokedFlowerSeeds"
                |> save
        seeds
                |> Seq.map (fun x -> KeyValuePair(x.Seed, x.Main path)) 
                |> Dictionary
        
    /// <summary>
    /// Loads sunflower plugins from filesystem
    /// (needed directory: .../Plugins)
    /// </summary>
    member public this.LoadAllFlowerSeeds () =
        "Sunflower Kernel::LoadAllFlowerSeeds"
            |> save
        // C# core part
        // if (!Directory.Exists(seedPath))
        // {
        //     Debug.WriteLine("Plugins directory not found");
        //     return this;
        // }

        // foreach (var dllPath in Directory.EnumerateFiles(seedPath, "*.dll", SearchOption.AllDirectories))
        // {
        //     try
        //     {
        //         var assembly = Assembly.LoadFrom(dllPath);
                
        //         foreach (var type in assembly.GetTypes())
        //         {
        //             if (!typeof(IFlowerSeed).IsAssignableFrom(type))
        //             {
        //                 Debug.WriteLine($"Type {type} not assigned from {nameof(IFlowerSeed)}");
        //                 continue;
        //             }
                    
        //             // IFlowerSeed instance
        //             var plugin = (IFlowerSeed)Activator.CreateInstance(type)!;
                        
        //             // Add to hashset
        //             Seeds.Add(plugin);
        //             Debug.WriteLine($"Loaded plugin: {plugin.Seed}");
        //         }
        //     }
        //     catch (Exception ex)
        //     {
        //         Debug.WriteLine($"Error loading {dllPath}: {ex.Message}");
        //     }
        // }

        // F# rewritten part
        let dllPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");
        let parentType = typeof<IFlowerSeed>
        
        seeds <- Directory.GetFiles(dllPath, "*.dll")
            |> Seq.collect (fun file ->
                try
                    let assembly = Assembly.LoadFrom(file)
                    assembly.GetTypes()
                with _ -> [||])
            |> Seq.filter (fun t ->
                t.IsClass &&
                not t.IsAbstract &&
                t.IsAssignableTo(parentType))
            // Attribute
            |> Seq.choose (fun t ->
                let attr = t.GetCustomAttribute<FlowerSeedContractAttribute>()
                if attr.MajorVersion = majorVersion then
                    try
                        if attr.MinorVersion <> minorVersion then
                            $"[!]: {t.Name}#{attr.MajorVersion}.{attr.MinorVersion}.{attr.BuildVersion} differs with {majorVersion}.{minorVersion}.{buildVersion}"
                            |> save
                           
                        Activator.CreateInstance(t) :?> IFlowerSeed |> Some
                    with _ -> None
                else None
            )
            |> Seq.toList
        this
    /// <summary>
    /// Pointer to storage of all loaded
    /// and initialized (activated)
    /// sunflower plugins interfaces
    /// </summary>    
    member public this.Seeds with get () = List seeds
    member public this.Messages with get () = List messages
    
    member public this.UnloadUnusedFlowerSeeds =
        "Sunflower Kernel::UnloadUnusedFlowerSeeds"
            |> save
        
        seeds <- seeds
            |> List.where (fun x -> x.Status.IsResultExists)
            |> List.distinct
            |> Seq.toList
            
        this
    /// <summary>
    /// Updates <see cref="Seeds"/> collection
    /// by targeting file
    /// </summary>
    /// <param name="path">targeting file</param>
    member public this.UpdateAllInvokedFlowerSeeds(path) =
        "Sunflower Kernel::UpdateAllInvokedFlowerSeeds"
            |> save
        try
            seeds
            |> Seq.toList
            |> List.iter (fun x ->
                x.Main path
                |> ignore )
        with
        | kernel ->
            $"Sunflower Kernel::STOP\r\n >> {kernel |> string}"
                |> save
        
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
    static member public CreateInstance () : FlowerSeedManager =
        FlowerSeedManager()
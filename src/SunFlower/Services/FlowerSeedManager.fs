namespace SunFlower.Services

open System
open System.Collections.Generic
open System.Diagnostics
open System.IO
open System.Reflection
open SunFlower.Abstractions
//
// CoffeeLake 2024-2025
// This code licensed under MIT. Please see GitHub repo documentation
//
// @creator: atolstopyatov2017@vk.com
//
type FlowerSeedManager() =
    let mutable seeds : List<IFlowerSeed> = List<IFlowerSeed>()
    
    interface IFlowerSeedManager with
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
        member this.GetAllInvokedFlowerSeeds(path) =
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
                    |> Console.WriteLine
            seeds
                    |> Seq.map (fun x -> KeyValuePair(x.Seed, x.Main path)) 
                    |> Dictionary
            
        /// <summary>
        /// Loads sunflower plugins from filesystem
        /// (needed directory: .../Plugins)
        /// </summary>
        member this.LoadAllFlowerSeeds = 
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
            
            this
        /// <summary>
        /// Pointer to storage of all loaded
        /// and initialized (activated)
        /// sunflower plugins interfaces
        /// </summary>    
        member this.Seeds
            with get () = seeds
        member this.UnloadUnusedFlowerSeeds =
            "Sunflower Kernel::UnloadUnusedFlowerSeeds"
                |> Console.WriteLine
            
            seeds <- seeds
                |> Seq.toList
                |> List.where (fun x -> x.Status.IsResultExists)
                |> List.distinct
                |> List
                
            this
        /// <summary>
        /// Updates <see cref="Seeds"/> collection
        /// by targeting file
        /// </summary>
        /// <param name="path">targeting file</param>
        member this.UpdateAllInvokedFlowerSeeds(path) =
            "Sunflower Kernel::UpdateAllInvokedFlowerSeeds"
                |> Console.WriteLine
            try
                seeds
                |> Seq.toList
                |> List.iter (fun x ->
                    x.Main (path)
                    |> ignore )
            with
            | :? Exception as kernel ->
                $"Sunflower Kernel::STOP\r\n >> {kernel |> string}"
                    |> Console.WriteLine
                 // |> failwith kernel |> string // just wait.
            
            this
    
    /// <summary>
    /// F# makes more strongly inherit process
    /// than C#. This is a Seeds { set; } property
    /// because Seeds { get; +set; } not satisfied
    /// with IFlowerSeedManager rules.
    /// </summary>
    member this.SeedsInit
        with set s = seeds <- s
        
    /// <summary>
    /// Makes temporary instance for FlowerSeedManager
    /// </summary>
    static member CreateInstance () : FlowerSeedManager =
        FlowerSeedManager()
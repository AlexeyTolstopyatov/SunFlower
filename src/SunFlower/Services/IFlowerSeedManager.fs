namespace SunFlower.Services

//
// CoffeeLake 2024-2025
// This code licensed under MIT. Please see GitHub repo documentation
//
// @creator: atolstopyatov2017@vk.com
//


open System
open System.Collections.Generic
open SunFlower.Abstractions

type IFlowerSeedManager = interface
    /// <summary>
    /// Set of SunFlower external DLLs.
    /// </summary>
    abstract member Seeds : List<IFlowerSeed>
    /// <summary>
    /// Checks and collects all Flower seeds to HashSet
    /// </summary>
    abstract member LoadAllFlowerSeeds : IFlowerSeedManager
    /// <summary>
    /// If loaded plugin has zero result -> removes it from list
    /// </summary>
    /// <returns></returns>
    abstract member UnloadUnusedFlowerSeeds : IFlowerSeedManager
    /// <summary>
    /// Call every loaded plugins EntryPoint for current file
    /// </summary>
    /// <param name="path">Targeting file</param>
    /// <returns></returns>
    abstract member UpdateAllInvokedFlowerSeeds : path : String -> IFlowerSeedManager
    /// <summary>
    /// Returns entry table (status of all Main procedures)
    /// </summary>
    /// <param name="path">Targeting file</param>
    /// <returns></returns>
    abstract member GetAllInvokedFlowerSeeds : path : String -> Dictionary<String, Int32>
    /// <summary>
    /// Kernel storage of messages
    /// </summary>
    abstract member Messages : List<string>
    end

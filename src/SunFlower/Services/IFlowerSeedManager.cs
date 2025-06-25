using SunFlower.Abstractions;

namespace SunFlower.Services;

public interface IFlowerSeedManager
{
    /// <summary>
    /// Set of SunFlower external DLLs.
    /// </summary>
    List<IFlowerSeed> Seeds { get; set; }
    /// <summary>
    /// Checks and collects all Flower seeds to HashSet
    /// </summary>
    IFlowerSeedManager LoadAllFlowerSeeds();
    /// <summary>
    /// If loaded plugin has zero result -> removes it from list
    /// </summary>
    /// <returns></returns>
    IFlowerSeedManager UnloadUnusedSeeds();
    /// <summary>
    /// Returns entry table (status of all Main prcedures)
    /// </summary>
    /// <param name="targetingFile"></param>
    /// <returns></returns>
    Dictionary<string, int> GetAllInvokedSeedResults(string targetingFile);

}
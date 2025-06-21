using SunFlower.Abstractions;

namespace SunFlower.Services;

public interface IFlowerSeedConnect
{
    /// <summary>
    /// Set of SunFlower external DLLs.
    /// </summary>
    HashSet<IFlowerSeed> Seeds { get; set; }
    /// <summary>
    /// Checks and collects all Flower seeds to HashSet
    /// </summary>
    void Initialize();
}
using System.Diagnostics;
using System.Reflection;
using SunFlower.Abstractions;

namespace SunFlower.Services;

public class FlowerSeedManager : IFlowerSeedManager
{
    /// <summary>
    /// Singleton instance of plugins manager
    /// </summary>
    /// <returns></returns>
    public static FlowerSeedManager CreateInstance()
    {
        return new FlowerSeedManager();
    }
    /// <summary>
    /// Loaded plugin interfaces
    /// </summary>
    public List<IFlowerSeed> Seeds { get; set; } = [];
    /// <summary>
    /// Loads sunflower plugins from filesystem
    /// (needed directory: .../Plugins)
    /// </summary>
    public IFlowerSeedManager LoadAllFlowerSeeds()
    {
        Seeds = []; // again!
        var seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        if (!Directory.Exists(seedPath))
        {
            Debug.WriteLine("Plugins directory not found");
            return this;
        }

        foreach (var dllPath in Directory.EnumerateFiles(seedPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                var assembly = Assembly.LoadFrom(dllPath);
                
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(IFlowerSeed).IsAssignableFrom(type))
                    {
                        Debug.WriteLine($"Type {type} not assigned from {nameof(IFlowerSeed)}");
                        continue;
                    }
                    
                    // IFlowerSeed instance
                    var plugin = (IFlowerSeed)Activator.CreateInstance(type)!;
                        
                    // Add to hashset
                    Seeds.Add(plugin);
                    Debug.WriteLine($"Loaded plugin: {plugin.Seed}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error loading {dllPath}: {ex.Message}");
            }
        }

        return this;
    }
    /// <summary>
    /// Executes all seeds and returns status table
    /// for every seed.
    /// </summary>
    public Dictionary<string, int> GetAllInvokedSeedResults(string targetingFile)
    {
        var results = new Dictionary<string, int>();
        
        foreach (var plugin in Seeds)
        {
            try
            {
                // Main invoke
                var result = plugin.Main(targetingFile);
                results.Add(plugin.Seed, result);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Plugin {plugin.Seed} failed: {ex.Message} \n\n {plugin} \n");
            }
        }
        
        return results;
    }
    /// <summary>
    /// Updates <see cref="Seeds"/> collection
    /// by targeting file
    /// </summary>
    /// <param name="path">targeting file</param>
    public FlowerSeedManager UpdateAllInvokedFlowerSeeds(string path)
    {
        foreach (var seed in Seeds)
        {
            try
            {
                seed.Main(path);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Plugin {seed.Seed}\r\nSunFlower chain: {e}");
            }
        }

        return this;
    }
    /// <summary>
    /// All plugins having 0 result <c>IsResultExists</c> not true
    /// will be removed from list
    /// </summary>
    /// <returns>Action result</returns>
    public IFlowerSeedManager UnloadUnusedSeeds()
    {
        Seeds = Seeds
            .Where(s => s.Status.IsResultExists)
            .Distinct()
            .ToList();
        return this;
    }
}
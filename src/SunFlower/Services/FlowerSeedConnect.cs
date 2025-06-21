using System.Data;
using System.Diagnostics;
using System.Reflection;
using SunFlower.Abstractions;
using FlowerSeedResult = SunFlower.Abstractions.FlowerSeedResult;

namespace SunFlower.Services;

public class FlowerSeedConnect : IFlowerSeedConnect
{
    public HashSet<IFlowerSeed> Seeds { get; set; } = [];

    public void Initialize()
    {
        Seeds = []; // again!
        string seedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins");

        if (!Directory.Exists(seedPath))
        {
            Debug.WriteLine("Plugins directory not found");
            return;
        }

        foreach (string dllPath in Directory.EnumerateFiles(seedPath, "*.dll", SearchOption.AllDirectories))
        {
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllPath);
                
                foreach (Type type in assembly.GetTypes())
                {
                    if (!typeof(IFlowerSeed).IsAssignableFrom(type))
                    {
                        Debug.WriteLine($"Type {type} not assigned from {nameof(IFlowerSeed)}");
                        continue;
                    }
                    
                    // IFlowerSeed instance
                    IFlowerSeed plugin = (IFlowerSeed)Activator.CreateInstance(type)!;
                        
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
    }
    
    /// <summary>
    /// Executes all seeds and returns status table
    /// for every seed.
    /// </summary>
    public Dictionary<string, int> GetEntryPointTables(string targetingFile)
    {
        var results = new Dictionary<string, int>();
        
        foreach (IFlowerSeed plugin in Seeds)
        {
            try
            {
                // Main invoke
                int result = plugin.Main(targetingFile);
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
    /// Invoke sunflower seed "by hand" for targeting file
    /// </summary>
    /// <param name="path">file target</param>
    /// <param name="seed">targeting seed</param>
    /// <returns>Action result</returns>
    public FlowerSeedResult InvokeFlowerSeed(IFlowerSeed seed, string path)
    {
        seed.Main(path);
        return seed.Result;
    }
}
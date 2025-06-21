using System.Data;

namespace SunFlower.Abstractions;

[Serializable]
public sealed class FlowerSeedResult : MarshalByRefObject
{
    /// <summary>
    /// Holds on state of seed's usage.
    /// Seeds HashSet contains all plugins (enabled and don't)
    /// but you can disable anybody "by hand"
    /// </summary>
    public bool IsEnabled { get; set; }
    /// <summary>
    /// If result table is null or
    /// Count of results is zero -> connection failed
    /// returns false. 
    /// </summary>
    public bool IsResultExists => Result.Length != 0;
    /// <summary>
    /// Results array of external DLL
    /// </summary>
    public DataTable[] Result { get; set; } = [];
}
using System.Data;
//
// CoffeeLake (C) 2025
// 

namespace SunFlower.Abstractions;

/// <summary>
/// Status entity. All operations result
/// contains here in <see cref="DataTable"/> array.
///
/// Special flags i.e. <c>IsEnabled</c> switch
/// in plugin's body for next usage (or not).
/// </summary>
[Serializable]
public sealed class FlowerSeedStatus : MarshalByRefObject
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
    /// <summary>
    /// Stores last exception or exceptions chain
    /// </summary>
    public Exception? LastError { get; set; }
}
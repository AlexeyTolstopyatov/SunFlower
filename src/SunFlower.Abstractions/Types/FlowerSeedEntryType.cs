namespace SunFlower.Abstractions.Types;
using System.Data;

/// <summary>
/// Types of returning values from plugins.
/// </summary>
public enum FlowerSeedEntryType
{
    /// <summary>
    /// If one of results has this type,
    /// it must be unboxed to <see cref="DataTable"/>[]
    /// </summary>
    DataTables,
    /// <summary>
    /// If one of results has this type it must
    /// be unboxed to <see cref="byte"/>[]
    /// </summary>
    RawBytes,
}
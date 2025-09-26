namespace SunFlower.Abstractions.Types

open System

///
/// CoffeeLake 2025
/// this code licensed under (see GitHub repo) license
///
/// @creator: atolstopyatov2017@vk.com
///

///
/// Main type mask for boxed data
/// You can unbox System.Object result using this enumerator
/// 
[<Flags>]
type FlowerSeedEntryType =
    /// <summary>
    /// Specifies the unknown or null result of operation
    /// </summary>
    | Empty = 0
    /// <summary>
    /// If one of results has this type,
    /// it must be unboxed to <see cref="DataTable"/>[]
    /// </summary>
    | DataTables = 1
    /// <summary>
    /// If one of results has this type it must
    /// be unboxed to <see cref="byte"/>[]
    /// </summary>
    | Bytes = 2
    /// <summary>
    /// <see cref="List{String}"/> instance which contains all data
    /// </summary>
    | Strings = 3
    /// <summary>
    /// List(Of <see cref="Region"/>)s.
    /// Region contains Header-string, String of content and DataTable
    /// </summary>
    | Regions = 4

[<Class>]
type FlowerSeedResult(resultType: FlowerSeedEntryType, result: Object) = class   
    let mutable boxedResult : Object = 0
    new (r: FlowerSeedEntryType) =
        FlowerSeedResult(r, 0)
    /// <summary>
    /// Stores type of Boxed result from flower's seed
    /// (external plugin result which will be dereferenced by client)
    /// </summary>
    member val public Type = resultType
    /// <summary>
    /// Points to boxed result (universal .NET Object type)
    /// which must dereferenced and unboxed by following Type 
    /// </summary>
    member public _.BoxedResult
        with get () = boxedResult
        and set result = boxedResult <- result
    
    end
    
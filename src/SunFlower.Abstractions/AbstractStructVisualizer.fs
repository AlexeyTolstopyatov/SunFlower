namespace SunFlower.Abstractions

open System.Data
open SunFlower.Abstractions.Types


[<AbstractClass>]
type AbstractStructVisualizer<'TStruct>(s : 'TStruct) =
    /// <summary>
    /// Holds the structure given from header
    /// </summary>
    member _._struct : 'TStruct = s
    /// <summary>
    /// Actually builds DataTable instance 
    /// </summary>
    abstract member ToDataTable : unit -> DataTable
    /// <summary> 
    /// Makes new Region instance using ToDataTable function
    /// </summary>
    abstract member ToRegion : unit -> Region
    
    


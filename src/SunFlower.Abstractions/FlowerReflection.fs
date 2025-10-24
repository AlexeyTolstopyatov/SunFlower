namespace SunFlower.Abstractions

module FlowerReflection = 
    // CoffeeLake 2025
    // 
    // Module represents API for deserializing safe types
    // to DataTable objects through standard .NET reflection
    //
    // SunFlower datatypes declared like machine word sizes
    //      :1 | BYTE   | byte/sbyte   | 
    //      :2 | WORD   | UInt16/Int16 | 
    //      :4 | DWORD  | UInt32/Int32 | 
    //      :8 | QWORD  | UInt64/Int64 |
    //      :s | <'Any> | String       | Any type of string (e.g. NET String)
    //      :s_| BYTE[] | Char[]       | NON-Terminated ASCII string
    //      :sz| BYTE[] | Char[]       | Terminated ASCII String
    //      :ps| BYTE[] | Char[]       | Pascal String
    //      :bs| WORD[] | String       | Binary String [OR] UTF-16 .NET String
    //      :f | BYTE   | Boolean      | Flag
    //
    // Column (type) declaration:
    //      FlowerReport.ForColumn("lpExternalTable", typeof(int)) -> "lpExternalTable:4"
    /// <summary>
    /// Accepts only types by value. Throws exceptions.
    /// Uses standard .NET reflection for types deserialization. 
    /// </summary>
    [<CompiledName "ToDataTable">]
    let to_data_table<'TSafe>() =
        -21
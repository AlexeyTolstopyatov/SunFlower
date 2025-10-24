namespace SunFlower.Abstractions

open System
open System.Data
open System.Reflection
open Microsoft.FSharp.Collections

module FlowerReflection = 
    // CoffeeLake 2025
    // 
    // Module represents API for deserializing safe types
    // to DataTable objects through standard .NET reflection
    //
    // SunFlower datatypes declared like machine word sizes
    // All strings I see like by-value types. Not LPCSTR LPSTR and any ULONG pointer types
    //      :1 | BYTE   | byte/sbyte   | 
    //      :2 | WORD   | UInt16/Int16 | 
    //      :4 | DWORD  | UInt32/Int32 | 
    //      :8 | QWORD  | UInt64/Int64 |
    //      :s |        | String       | Any type of string (e.g. NET String)
    //      :s_| BYTE[] | Char[]       | NON-Terminated ASCII string
    //      :sz| BYTE[] | Char[]       | Terminated ASCII String
    //      :ps| BYTE[] | Char[]       | Pascal String
    //      :bs| WORD[] | String       | Binary String [OR] UTF-16 .NET String
    //      :f | BYTE   | Boolean      | Flag
    //      :dt|        | DateTime     | COR DateTime container or raw timestamp
    //      :t |        | struct/class | Complex unknown object (meant "type")
    //
    // Column (type) declaration:
    //      FlowerReport.ForColumn("lpExternalTable", typeof(int)) -> "lpExternalTable:4"
    /// <summary>
    /// Accepts only types by value. Throws exceptions.
    /// Uses standard .NET reflection for types deserialization. 
    /// </summary>
    [<CompiledName "GetNameValueTable">]
    let get_nv_table<'TSafe> (inst: 'TSafe) : DataTable =
        let dt = new DataTable()
        dt.Columns.Add("Name", typeof<string>) |> ignore
        dt.Columns.Add("Type", typeof<string>) |> ignore
        dt.Columns.Add("Value", typeof<string>) |> ignore
        
        let typ = typeof<'TSafe>
        
        let properties =
            typ.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        
        let fields =
            typ.GetFields(BindingFlags.Public ||| BindingFlags.Instance)
        
        let get_type_enum (t: Type) =
            match t with // guards and generics
            | _ when t = typeof<byte> -> FlowerType.U1
            | _ when t = typeof<int16> -> FlowerType.U2
            | _ when t = typeof<int32> -> FlowerType.U4
            | _ when t = typeof<int64> -> FlowerType.U8
            | _ when t = typeof<bool> -> FlowerType.Flag
            | _ when t = typeof<string> -> FlowerType.AnyStr
            | _ when t = typeof<DateTime> -> FlowerType.AnyStr
            | _ -> FlowerType.AnyStr
        
        let get_value_string (value: obj) =
            match value with
            | null -> String.Empty
            | :? int8 as b -> $"0x{b:X2}"
            | :? int16 as w -> $"0x{w:X4}"
            | :? int32 as dw -> $"0x{dw:X8}"
            | :? int64 as qw -> $"0x{qw:X16}"
            | :? DateTime as dt -> dt.ToString("yyyy-MM-dd HH:mm:ss")
            | _ -> value.ToString()
        
        for prop in properties do
            if prop.CanRead then
                let value = prop.GetValue(inst)
                let flt = get_type_enum prop.PropertyType
                let type_str = FlowerReport.for_column_fl(prop.Name, flt)
                dt.Rows.Add([| prop.Name; type_str; get_value_string value |]) |> ignore
        
        for field in fields do
            let value = field.GetValue(inst)
            let flt = get_type_enum field.FieldType
            let typeString = FlowerReport.for_column_fl(field.Name, flt)
            dt.Rows.Add([| field.Name; typeString; get_value_string value |]) |> ignore
        
        dt
        
    /// <summary>
    /// Function iterates list of ONLY ONE typed-object
    /// and ignores casting from abstract-classes.
    ///
    /// YOU MUST REMEMBER IT LIKE YOUR NAME.
    /// </summary>
    /// <param name="inst"></param>
    [<CompiledName "GetListTable">]
    let get_list_table (inst: List<'TSafe>) =
        
        0
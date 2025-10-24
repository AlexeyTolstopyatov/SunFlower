namespace SunFlower.Abstractions

open System
open System.Collections.Generic
open System.Data
open System.Reflection
open SunFlower.Abstractions

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
        dt.Columns.Add("Value", typeof<string>) |> ignore
        
        let typ = typeof<'TSafe> // how to trait it like: ... where TSafe : struct 
        
        let properties =
            typ.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
        
        let fields =
            typ.GetFields(BindingFlags.Public ||| BindingFlags.Instance)
        
        let get_type_enum (t: Type) =
            match t with // guards and generics
            | _ when t = typeof<Byte> -> FlowerType.U1
            | _ when t = typeof<SByte> -> FlowerType.U1
            | _ when t = typeof<Int16> -> FlowerType.U2
            | _ when t = typeof<UInt16> -> FlowerType.U2
            | _ when t = typeof<UInt16> -> FlowerType.U4
            | _ when t = typeof<Int32> -> FlowerType.U4
            | _ when t = typeof<Int64> -> FlowerType.U8
            | _ when t = typeof<UInt64> -> FlowerType.U8
            | _ when t = typeof<bool> -> FlowerType.Flag
            | _ when t = typeof<string> -> FlowerType.AnyStr
            | _ when t = typeof<DateTime> -> FlowerType.AnyStr
            | _ -> FlowerType.AnyStr
        
        let get_value_string (value: obj) =
            match value with
            | null -> String.Empty
            | :? UInt32
            | :? Int32 as dw -> $"0x{dw:X8}"
            | :? Byte
            | :? SByte as b -> $"0x{b:X2}"
            | :? UInt16
            | :? Int16 as w -> $"0x{w:X4}"
            | :? UInt64
            | :? Int64 as qw -> $"0x{qw:X16}"
            | :? DateTime as dt -> dt.ToString("yyyy-MM-dd HH:mm:ss")
            | _ -> value.ToString()
        
        for prop in properties do
            if prop.CanRead then
                let value = prop.GetValue(inst)
                let flt = get_type_enum prop.PropertyType
                let type_str = FlowerReport.for_column_fl(prop.Name, flt)
                dt.Rows.Add(type_str, get_value_string(value)) |> ignore
        
        for field in fields do
            let value = field.GetValue(inst)
            let flt = get_type_enum(field.FieldType)
            let type_str = FlowerReport.for_column_fl(field.Name, flt)
            dt.Rows.Add(type_str, get_value_string(value)) |> ignore
        
        dt
        
    /// <summary>
    /// Trait #1: Function iterates list of ONLY ONE typed-object
    /// and ignores casting from abstract-classes.
    ///
    /// Trait #2: Target Class must be Data-class or model, not
    /// 
    /// YOU MUST REMEMBER IT LIKE YOUR NAME.
    /// </summary>
    /// <param name="items">COR List of objects saved after deserialization</param>
    [<CompiledName "ListToDataTable">]
    let list_to_data_table<'TSafe> (items: IEnumerable<'TSafe>) : DataTable =
        let dt = new DataTable("CollectionData")
        let item_type = typeof<'TSafe>
        
        // empty list -> empty table with columns
        if items = null || not (items.GetEnumerator().MoveNext()) then
            // make Columns using 'TSafe
            let properties = item_type.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
            let fields = item_type.GetFields(BindingFlags.Public ||| BindingFlags.Instance)
            
            for prop in properties do
                if prop.CanRead then
                    dt.Columns.Add(prop.Name, typeof<string>) |> ignore
            
            for field in fields do
                dt.Columns.Add(field.Name, typeof<string>) |> ignore
        else
            let enumerator = items.GetEnumerator()
            enumerator.MoveNext() |> ignore
            let first_item = enumerator.Current
            
            let properties = item_type.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
            let fields = item_type.GetFields(BindingFlags.Public)
            
            for prop in properties do
                if prop.CanRead then
                    dt.Columns.Add(prop.Name, typeof<string>) |> ignore
            
            for field in fields do
                dt.Columns.Add(field.Name, typeof<string>) |> ignore
            
            dt.Columns.Add("_Index", typeof<int>) |> ignore
            
            // rows with data
            let rec reset_and_iterate (items: IEnumerable<'TSafe>) =
                let enumerator = items.GetEnumerator()
                let mutable index = 0
                while enumerator.MoveNext() do
                    let item = enumerator.Current
                    let row = dt.NewRow()
                    
                    for prop in properties do
                        if prop.CanRead then
                            let value = prop.GetValue(item)
                            row[prop.Name] <- if value = null then String.Empty else value.ToString()
                    
                    for field in fields do
                        let value = field.GetValue(item)
                        row[field.Name] <- if value = null then String.Empty else value.ToString()
                    
                    row["_Index"] <- index
                    dt.Rows.Add(row)
                    index <- index + 1

            reset_and_iterate(items)
        dt

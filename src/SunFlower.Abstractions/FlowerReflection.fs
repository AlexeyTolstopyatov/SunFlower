namespace SunFlower.Abstractions

open System
open System.Collections.Generic
open System.Data
open System.Reflection
open System.Text
open SunFlower.Abstractions
open SunFlower.Abstractions.Pointers


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
    //      :ps| BYTE[] | Byte[]       | Pascal String
    //      :bs| WORD[] | String       | Binary String [OR] UTF-16 .NET String
    //      :ws| WORD[] | UInt16[]     | Unicode (wide)String (wchar_t/WCHAR)
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
    [<CompiledName "DictionaryDataTable">]
    let dictionaryDataTable<'TSafe> (inst: 'TSafe) : DataTable =
        let dt = new DataTable()
        dt.Columns.Add("Key", typeof<string>) |> ignore
        dt.Columns.Add("Value", typeof<string>) |> ignore

        let typ = typeof<'TSafe> // how to trait it like: ... where TSafe : IEnumerable

        let properties = typ.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)

        let fields = typ.GetFields(BindingFlags.Public ||| BindingFlags.Instance)

        let getTypeEnum (t: Type) =
            match t with // guards and generics
            | _ when t = typeof<Byte> -> FlowerType.U1
            | _ when t = typeof<SByte> -> FlowerType.U1
            | _ when t = typeof<Int16> -> FlowerType.U2
            | _ when t = typeof<UInt16> -> FlowerType.U2
            | _ when t = typeof<UInt32> -> FlowerType.U4
            | _ when t = typeof<Int32> -> FlowerType.U4
            | _ when t = typeof<Int64> -> FlowerType.U8
            | _ when t = typeof<UInt64> -> FlowerType.U8
            | _ when t = typeof<bool> -> FlowerType.Flag
            | _ when t = typeof<Char[]> -> FlowerType.CStr
            | _ when t = typeof<Byte[]> -> FlowerType.PascalStr
            | _ when t = typeof<UInt16[]> -> FlowerType.WStr
            | _ when t = typeof<string> -> FlowerType.AnyStr
            | _ when t = typeof<DateTime> -> FlowerType.AnyStr
            | _ -> FlowerType.AnyStr

        let getValueString (value: obj) =
            match value with
            | null -> String.Empty
            // | :? int8
            // | :? int16
            // | :? int32
            // | :? int64
            // | :? uint8
            // | :? uint16
            // | :? uint32
            // | :? uint64 -> $"{value}" --> type redefinition doesn't work right :D!!!
            | :? U8Ptr
            | :? I8Ptr as b -> $"0x{b:X2}"
            | :? U16Ptr
            | :? I16Ptr as w -> $"0x{w:X4}"
            | :? U32Ptr
            | :? I32Ptr as dw -> $"0x{dw:X8}"
            | :? U64Ptr
            | :? I64Ptr as qw -> $"0x{qw:X16}"
            | :? DateTime as dt -> dt.ToString("yyyy-MM-dd HH:mm:ss")
            | :? array<Char> as sz -> sz |> String |> FlowerReport.safeString
            | :? array<Byte> as ps -> ps |> Encoding.ASCII.GetString |> FlowerReport.safeString
            | _ -> value.ToString()

        for prop in properties do
            if prop.CanRead then
                let value = prop.GetValue(inst)
                let flt = getTypeEnum prop.PropertyType
                let typeStr = FlowerReport.forColumnFl (prop.Name, flt)
                dt.Rows.Add(typeStr, getValueString value) |> ignore

        for field in fields do
            let value = field.GetValue(inst)
            let flt = getTypeEnum field.FieldType
            let typeStr = FlowerReport.forColumnFl (field.Name, flt)
            dt.Rows.Add(typeStr, getValueString value) |> ignore

        dt

    /// <summary>
    /// Trait #1: Function iterates list of ONLY ONE typed-object
    /// and ignores casting from abstract-classes.
    ///
    /// Trait #2: Target Class must be Data-class or model.
    ///
    /// YOU MUST REMEMBER IT LIKE YOUR NAME.
    /// </summary>
    /// <param name="items">COR List of objects saved after deserialization</param>
    [<CompiledName "ListToDataTable">]
    let arrayToDataTable<'T> (items: IEnumerable<'T>) : DataTable =
        let getValueString (value: obj) =
            match value with
            | :? U8Ptr
            | :? I8Ptr as b -> $"0x{b:X2}"
            | :? U16Ptr
            | :? I16Ptr as w -> $"0x{w:X4}"
            | :? U32Ptr
            | :? I32Ptr as dw -> $"0x{dw:X8}"
            | :? U64Ptr
            | :? I64Ptr as qw   -> $"0x{qw:X16}"
            | :? DateTime as dt -> dt.ToString("yyyy-MM-dd HH:mm:ss")
            | :? String as str  -> str |> FlowerReport.safeString
            | :? array<Char> as sz -> sz |> String |> FlowerReport.safeString
            | :? array<Byte> as ps -> ps |> Encoding.ASCII.GetString |> FlowerReport.safeString
            | _ -> value.ToString()

        let dt = new DataTable($"serialized${typeof<'T>}")
        let itemType = typeof<'T>

        let underlyingType (t: Type) =
            if t.IsGenericType && t.GetGenericTypeDefinition() = typedefof<Option<_>> then
                t.GetGenericArguments()[0]
            else
                t

        // First item columns construct
        let firstItem =
            if items <> null then
                Some(items.GetEnumerator().MoveNext())
            else
                None

        match firstItem with
        | Some _ ->
            let properties =
                itemType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
                |> Array.filter _.CanRead

            let fields = itemType.GetFields(BindingFlags.Public ||| BindingFlags.Instance)

            for prop in properties do
                let _columnType = underlyingType prop.PropertyType
                dt.Columns.Add(prop.Name) |> ignore

            for field in fields do
                let _columnType = underlyingType field.FieldType
                dt.Columns.Add(field.Name) |> ignore

            dt.Columns.Add("#", typeof<int>) |> ignore

            items
            |> Seq.iteri (fun index item ->
                let row = dt.NewRow()

                for prop in properties do
                    try
                        let value = prop.GetValue(item)
                        row[prop.Name] <- getValueString value
                    with ex ->
                        row[prop.Name] <- DBNull.Value
                        printfn $"Error reading property %s{prop.Name}: %s{ex.Message}"

                for field in fields do
                    try
                        let value = field.GetValue(item)
                        row[field.Name] <- getValueString value
                    with ex ->
                        row[field.Name] <- DBNull.Value
                        printfn $"Error reading field %s{field.Name}: %s{ex.Message}"

                row["#"] <- index
                dt.Rows.Add(row))
        | None ->
            let properties =
                itemType.GetProperties(BindingFlags.Public ||| BindingFlags.Instance)
                |> Array.filter _.CanRead

            let fields = itemType.GetFields(BindingFlags.Public ||| BindingFlags.Instance)

            for prop in properties do
                let columnType = underlyingType prop.PropertyType
                dt.Columns.Add(prop.Name, columnType) |> ignore

            for field in fields do
                let columnType = underlyingType field.FieldType
                dt.Columns.Add(field.Name, columnType) |> ignore

            dt.Columns.Add("#", typeof<int>) |> ignore

        dt
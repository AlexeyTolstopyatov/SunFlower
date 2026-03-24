namespace SunFlower.Services

open System
open System.IO
open System.Text.Json
open System.Threading.Tasks

// CoffeeLake (C) 2024-2026
// MIT
// 
// JSON service file 
type JsonServiceState<'T> = {
    FilePath : string
    Data : Option<list<'T>>
}

module JsonService =
    [<Literal>]
    let RECENT_TABLE: string = "recent"
    let instance = {
        FilePath = ""
        Data = None
    }
    /// <summary>
    /// Remember the file
    /// </summary>
    /// <param name="path"></param>
    /// <param name="state"></param>
    let touch path state =
        { state with FilePath = path }
    /// <summary>
    /// Read data from the type
    /// </summary>
    /// <param name="state"></param>
    let read state =
        if File.Exists state.FilePath then
            let json = File.ReadAllText state.FilePath
            let data = JsonSerializer.Deserialize<'T list>(json)
            { state with Data = Some data }
        else
            { state with Data = Some [] }
    /// <summary>
    /// Add new type instance. If data is missing -> read fsobject then append
    /// </summary>
    /// <param name="record"></param>
    /// <param name="state"></param>
    let addRecord record state =
        match state.Data with
        | Some existing -> { state with Data = Some (record :: existing) }
        | None -> 
            let newState = read state
            match newState.Data with
            | Some existing -> { newState with Data = Some (record :: existing) }
            | None -> failwith $"Unable to load data by {instance.FilePath}"
    /// <summary>
    /// Write type instance
    /// </summary>
    /// <param name="state"></param>
    let write state =
        match state.Data with
        | Some data ->
            let json = JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true))
            File.WriteAllText(state.FilePath, json)
            state
        | None -> failwith "No data to write."
    /// <summary>
    /// Loads file by given name
    /// </summary>
    /// <param name="name"></param>
    let load<'T> (name: string) : 'T list =
        let path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", name + ".json")
        if File.Exists path then
            let json = File.ReadAllText path
            JsonSerializer.Deserialize<'T list>(json)
        else []
    /// <summary>
    /// Saves file by given name and JsonService data state
    /// </summary>
    /// <param name="name">file name without extension</param>
    /// <param name="data">list of objects</param>
    let save<'T> (name: string) (data: 'T list) =
        let json = JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true))
        File.WriteAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Registry", name + ".json"), json)
    /// <summary>
    /// Loads file by given name
    /// </summary>
    /// <param name="name">file name without extension</param>
    let loadAsync<'T> (name: string): Task<'T list> = task {
        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Registry", name + ".json")
        
        match File.Exists path with
        | false -> return []
        | true ->
        // Return deserialized list of target type
        let! json = File.ReadAllTextAsync path
        return JsonSerializer.Deserialize<'T list> json
    }
    /// <summary>
    /// Saves file by given name and JsonService data state
    /// </summary>
    /// <param name="name">file name without extension</param>
    /// <param name="data">list of objects</param>
    let saveAsync<'T> (name: string) (data: 'T list) = task {
        let path = Path.Combine (AppDomain.CurrentDomain.BaseDirectory, "Registry", name + ".json")
        let json = JsonSerializer.Serialize(data, JsonSerializerOptions(WriteIndented = true))
        
        do! File.WriteAllTextAsync (path, json)
    }
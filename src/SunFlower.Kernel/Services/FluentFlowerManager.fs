namespace SunFlower.Kernel.Services

open System
open System.Collections.Concurrent
open System.Collections.Generic
open System.Data
open System.IO
open System.Linq
open System.Reflection
open System.Text
open System.Threading.Tasks
open SunFlower.Kernel
open SunFlower.Abstractions
open Microsoft.FSharp.Collections
open SunFlower.Kernel.Writers

//
// CoffeeLake 2025-*
// This code licensed under MIT. Please see GitHub repo documentation.
// @creator: atolstopyatov2017@vk.com
//

type public FlowerData =
    { instance: IFlower
      kind: FlowerTarget
      version: Version }

    member public me.Instance = me.instance
    member public me.Kind = me.kind
    member public me.Version = me.version

    /// <summary>
    /// Enumerate all properties/fields of [Flower] object which have a [Seed] metadata
    /// If given language entry is a public field or property instance, which has a [Seed]
    /// attribute, it will be included into flower seed collection.
    ///
    /// Basic class which implements IFlower interface doesn't have properties except the Flower name
    /// (IFlower.Name) 
    /// </summary>
    [<CompiledName "EnumerateFlowerSeeds">]
    member me.enumerateFlowerSeeds() =
        let props =
            me.instance.GetType().GetProperties(BindingFlags.Public ||| BindingFlags.Instance)

        let fields =
            me.instance.GetType().GetFields(BindingFlags.Public ||| BindingFlags.Instance)

        seq {
            for p in props do
                let attr = p.GetCustomAttribute<SeedAttribute>()

                try
                    let property = p.GetValue(me.instance)
                    if not attr.Skip then 
                        yield (attr.Name, attr.Description, property)
                with _ ->
                    ()

            for f in fields do
                // Attribute [Seed] can't be null because of default constructor defined?.
                // F# nullity check is strange sometimes.
                // Let nullity check will be in run-time
                let attr = f.GetCustomAttribute<SeedAttribute>()

                try
                    if not attr.Skip then
                        yield (attr.Name, attr.Description, f.GetValue(me.instance))
                with _ ->
                    ()
        }
        |> Seq.toList

    /// <summary>
    /// Given by <c>GetType().GetValue()</c> Object instance holds
    /// constraints and type metadata, but this is not used unfortunately in this code scope
    ///
    /// Language runtime doesn't unbox concrete type and
    /// generic methods from <c>FlowerReflection</c> don't apply
    /// given object instance as a generic type.
    ///
    /// That's why I need too much reflection
    /// </summary>
    /// <param name="t"></param>
    member private _.fromEnumerableDynamic(t: obj) =
        let elementType =
            let t = t.GetType()

            if t.IsArray then
                t.GetElementType()
            else
                match
                    t.GetInterfaces()
                    |> Seq.tryFind (fun i ->
                        i.IsGenericType && i.GetGenericTypeDefinition() = typedefof<IEnumerable<_>>)
                with
                | Some i -> i.GetGenericArguments()[0]
                | None -> typeof<obj>

        let method =
            Assembly
                .LoadFile(Path.Combine(AppContext.BaseDirectory, "SunFlower.Abstractions.dll"))
                .GetType("SunFlower.Abstractions.FlowerReflection")
                .GetMethod("ListToDataTable")
                .MakeGenericMethod([| elementType |])

        method.Invoke(null, [| t |]) :?> DataTable

    member private me.fromDynamic(t: obj) =
        let method =
            Assembly
                .LoadFile(Path.Combine(AppContext.BaseDirectory, "SunFlower.Abstractions.dll"))
                .GetType("SunFlower.Abstractions.FlowerReflection")
                .GetMethod("DictionaryDataTable")
                .MakeGenericMethod([| t.GetType() |])

        method.Invoke(null, [| t |]) :?> DataTable

    member me.asDataTable(t: obj) =
        match t.GetType().IsArray with
        | true -> me.fromEnumerableDynamic t
        | false -> me.fromDynamic t

    member me.render() =
        let builder = StringBuilder()
        
        match me.kind with
        | FlowerTarget.Data ->
            me.enumerateFlowerSeeds ()
            |> Seq.iter (fun (name, description, t) ->
                $"### {name}" |> builder.AppendLine |> ignore
                description |> builder.AppendLine |> ignore

                t
                |> me.asDataTable
                |> FlowerMarkdownWriter.formatTable
                |> builder.AppendLine
                |> ignore)
        | FlowerTarget.Code
        | FlowerTarget.Process ->
            me.enumerateFlowerSeeds ()
            |> Seq.iter (fun (_, _, instance) ->
                match instance.GetType().IsArray with
                | true ->
                    instance :?> IEnumerable<string>
                    |> Seq.iter (fun s ->
                        s
                        |> string
                        |> builder.AppendLine
                        |> ignore)
                | false -> instance.ToString()
                           |> builder.AppendLine
                           |> ignore 
                )
        | _ -> NotSupportedException("Sorry, given flower target not supported in this version") |> raise

        // Produce toxic waste
        builder.ToString()
///
/// SunFlower plugins manager with Fluent API for C#/VB.net client side
///
[<FlowerVersionContract(5, 0, 0)>]
type FluentFlowerManager() =
    let mutable flowers: CorList<FlowerData> = CorList()
    let mutable messages: ConcurrentBag<string> = ConcurrentBag()
    /// <summary>
    /// Writes message to Kernel messages storage (CorList of strings)
    ///
    /// Client can read this storage and make
    /// a verbose output or current user.
    ///
    /// </summary>
    /// <param name="str"></param>
    let send (str: string) : unit = messages.Add str

    let fromParentMetadata () =
        let parent = typeof<FluentFlowerManager>
        let version = parent.GetCustomAttribute<FlowerVersionContractAttribute>()

        Version(version.MajorVersion, version.MinorVersion, version.BuildVersion)

    let mutable parentVersion = Version()
    do parentVersion <- fromParentMetadata ()

    [<CompiledName "GetContract">]
    member public this.getContract() = parentVersion |> string

    [<CompiledName "ActivateAllAsync">]
    member this.activateAllAsync() =
        task {
            let pluginsPath = Path.Combine(AppContext.BaseDirectory, "Plugins")

            if not (Directory.Exists pluginsPath) then
                send "Load Error: /Plugins subdirectory is missing!"
                return ()

            let! assemblies =
                Directory.GetFiles(pluginsPath, "*.dll")
                |> Seq.map (fun path ->
                    task {
                        try
                            return Some(Assembly.LoadFrom(path))
                        with ex ->
                            send $"Load error: {Path.GetFileName(path)}: {ex.Message}"
                            return None
                    })
                |> Task.WhenAll

            let parentType = typeof<IFlower>

            let newFlowers =
                assemblies
                |> Seq.collect (fun asm ->
                    match asm with
                    | Some a -> try a.GetTypes() with _ -> [||] // <-- skill issue
                    | None -> [||])
                |> Seq.filter (fun t -> t.IsClass && not t.IsAbstract && t.IsAssignableTo(parentType))
                |> Seq.choose (fun t ->
                    let contract = t.GetCustomAttribute<FlowerVersionContractAttribute>()

                    // Nullity check literally denied!. But what if CustomAttribute is missing?!
                    if contract.MajorVersion <> parentVersion.Major then
                        send $"Version Error: {t.Name} has v.{contract.MajorVersion}.x, Expected v.{parentVersion.Major}!"
                        None
                    else
                        if contract.MinorVersion <> parentVersion.Minor then
                            send $"Version Warning: {t.Name} ({contract.MajorVersion}.{contract.MinorVersion}.{contract.BuildVersion}) differs with ({parentVersion})!"

                        try
                            let instance = Activator.CreateInstance(t) :?> IFlower
                            let kindAttr = t.GetCustomAttribute<FlowerAttribute>()

                            let version =
                                Version(contract.MajorVersion, contract.MinorVersion, contract.BuildVersion)

                            Some
                                { instance = instance
                                  kind = kindAttr.Target
                                  version = version }
                        with ex ->
                            send $"Activation Error: {t.Name}: {ex.Message}"
                            None)

            flowers.Clear()
            flowers.AddRange(newFlowers)
            send $"Done! Proceed {flowers.Count} entries."
            return ()
        }

    /// <summary>
    /// Pointer to storage of all loaded
    /// and initialized (activated)
    /// sunflower plugins interfaces
    /// </summary>
    member public this.LoadedFlowers = List flowers

    member public this.Messages = List messages

    /// <summary>
    /// Updates <see cref="Seeds"/> collection
    /// by targeting file
    /// </summary>
    /// <param name="filePath">targeting file</param>
    [<CompiledName "InitializeAllAsync">]
    member this.initializeAllAsync(filePath: string) =
        task {
            let compatible = flowers |> Seq.toList // |> Seq.filter (fun fd -> fd.instance.CanHandle(filePath)) |> Seq.toList

            if compatible.IsEmpty then
                send "Load Error: No such flowers loaded!"
                return ()
            // Future: limit it using Parallel.ForEach
            let tasks: Task<unit>[] =
                compatible
                |> Seq.map (fun fd ->
                    task {
                        try
                            do! fd.instance.CreateAsync(filePath)
                        with ex ->
                            //fd.IsInitialized <- false
                            //fd.Error <- Some ex.Message
                            send $"Flower Error: {fd.instance.Name}: {ex.Message}"
                    })
                |> Seq.toArray

            let! _ = Task.WhenAll(tasks) // not do! because got [unit] array instead of unit

            return ()
        }

    /// <summary>
    /// Recalls plugin by which matches by IFlower.Name property.
    /// results of it will be rewritten. (= updated).
    /// </summary>
    /// <param name="name"></param>
    /// <param name="filePath"></param>
    [<CompiledName "InitializeAsync">]
    member this.initializeAsync(name: string, filePath: string) =
        task {
            let compatible =
                flowers
                    .Where(fun s -> s.instance.Name = name)
                    .Select(Some)
                    .FirstOrDefault(None)

            match compatible with
            | None ->
                send "Load Error: IFlower.Name mismatch!"
                return ()
            | Some c ->
                // All right. Wake up, Neo
                try
                    do! c.instance.CreateAsync(filePath)
                with e ->
                    send $"Flower Error: {e}"
                    return ()
        }

    /// <summary>
    /// Makes temporary instance for manager
    /// </summary>
    [<CompiledName "CreateInstance">]
    static member public createInstance() : FluentFlowerManager = FluentFlowerManager()

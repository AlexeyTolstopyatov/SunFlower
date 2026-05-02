namespace SunFlower.Kernel.Services

open System
open SunFlower.Abstractions
open SunFlower.Kernel.Writers

/// <summary>
/// IFlowerSeed metadata container.
/// </summary>
type FlowerSeedData =
    { seed: IFlowerSeed
      kind: SeedTarget
      version: Version } with
    member this.render() =
        FlowerMarkdownWriter.write this.seed.Status.Results
    member this.lastErrorTrace () =
        this.seed.Status.LastError |> string
    member this.lastError () =
        this.seed.Status.LastError.Message
    member this.hasError () =
        this.seed.Status.LastError <> null

namespace SunFlower.Models

open System
open System.Buffers
open System.IO
open System.Text
open SunFlower.ViewModels
open Sunflower.Dasm
open Tmds.DBus.Protocol

type WorkspaceDisassemblerCallback =
    { name: string
      linearDecode: byte array -> string list
      recDecode: string * byte array * int array -> string }

[<Class>]
type WorkspaceDisassemblerModel() =
    inherit AvaloniaViewModel()
    let mutable modeIndex = 0
    let mutable modes = [|
        { name = "Intel 8086"
          linearDecode = I8086Decoder.decode
          recDecode = I8086Decoder.decodeRecursive };
        // { name = "Intel (80)186"; action = I80186Decoder.decode }
        // { name = "Intel (80)286"; action = I80286Decoder.decode }
        // { name = "Intel (80)386"; action = I80386Decoder.decode }
        // Later it will be [FlowerEmbeddedDisassembler] plugins or something the same and loading of disassemblers
        // became possible from another DLLs
    |]
    let mutable selectedVectors: int = 0
    let mutable interruptVectors = [||]
    do
        // Load available x86 Interrupt vectors
        let dbList = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Interrupt")
                     |> Directory.EnumerateFiles
                     |> Seq.map (fun path -> FileModel(Path.GetFileName path, path))
                     |> Seq.toArray
        interruptVectors <- dbList
        
    member this.Modes
        with get () = modes
        and set m =
            modes <- m
            this.OnPropertyChanged()
    member this.Vectors
        with get () = interruptVectors
        and set v =
            interruptVectors <- v
            this.OnPropertyChanged()
            
    member this.SelectedModeIndex
        with get () = modeIndex
        and set i =
            modeIndex <- i
            this.OnPropertyChanged()
            
    member this.SelectedVectors
        with get () = selectedVectors
        and set v =
            selectedVectors <- v
            this.OnPropertyChanged()
            
    
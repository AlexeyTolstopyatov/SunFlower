namespace SunFlower.Models

open System
open System.Text
open SunFlower.ViewModels
open Sunflower.Dasm

type WorkspaceDisassemblerCallback =
    { name: string
      action: byte array -> string list }

[<Class>]
type WorkspaceDisassemblerModel() =
    inherit AvaloniaViewModel()
    let mutable modeIndex = 0
    let mutable modes = [|
        { name = "Intel 8086"
          action = I8086Decoder.decode };
        // { name = "Intel (80)186"; action = I80186Decoder.decode }
        // { name = "Intel (80)286"; action = I80286Decoder.decode }
        // { name = "Intel (80)386"; action = I80386Decoder.decode }
        // Later it will be [FlowerEmbeddedDisassembler] plugins or something the same and loading of disassemblers
        // became possible from another DLLs
    |]
    
    member this.Modes
        with get () = modes
        and set m =
            modes <- m
            this.OnPropertyChanged()
    
    member this.SelectedModeIndex
        with get () = modeIndex
        and set i =
            modeIndex <- i
            this.OnPropertyChanged()
namespace Sunflower.Dasm

open System
open System.IO
open Sunflower.Dasm.Intel.Core

module public I80286Decoder =
    let get (interruptsPath: string) =
        let opcodesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opcodes286.json")
        let safePath = if File.Exists(interruptsPath) then Some(interruptsPath) else None 
        Decoder.create opcodesPath safePath ProcessorMode.I286

    let decode (bytes: byte[]) =
        let state = get("")
        Decoder.runLinear state bytes
                    |> Decoder.format
        
    let decodeWith (state: DecoderState) (bytes: byte []) =
        Decoder.runLinear state bytes
    
    let decodeRecursive (interruptsPath: string, bytes: byte array, offsets: int array)=
        let decoder = get(interruptsPath)
        let instructions, status, entrySet = Decoder.runRecursive decoder bytes offsets
        Decoder.formatWithLabels instructions bytes status entrySet
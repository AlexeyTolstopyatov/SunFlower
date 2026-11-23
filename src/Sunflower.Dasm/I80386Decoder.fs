namespace Sunflower.Dasm

open System
open System.Diagnostics
open System.IO
open Sunflower.Dasm.Intel.Core

module I80386Decoder =
    let get (interruptsPath: string, mode: ProcessorMode) =
        let opcodesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opcodes386.json")
        let safePath = if File.Exists(interruptsPath) then Some(interruptsPath) else None
        
        Decoder.create opcodesPath safePath mode
    
    let decodeWith (state: DecoderState) (bytes: byte []) =
        Decoder.runLinear state bytes
    
    let decodeRecursive (interruptsPath: string, bytes: byte array, offsets: int array, mode: ProcessorMode)=
        let decoder = get(interruptsPath, mode)
        let instructions, status, entrySet = Decoder.runRecursive decoder bytes offsets
        Decoder.formatWithLabels instructions bytes status entrySet
        
    let getInstructionSet (interruptPath: string, bytes: byte array, offsets: int array, mode: ProcessorMode) =
        let decoder = get(interruptPath, mode)
        
        Decoder.runRecursive decoder, bytes, offsets
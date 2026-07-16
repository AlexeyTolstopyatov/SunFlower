namespace Sunflower.Dasm

open System
open System.Collections.Generic
open System.IO
open System.Linq
open Microsoft.FSharp.Linq.RuntimeHelpers
open Sunflower.Dasm.Intel.Core

/// <summary>
/// This module represents setup of base intel disassembler for I8086 only!
/// For I186 disassembler use <see cref="I80186Decoder"/>
/// </summary>
module I8086Decoder =
    let get (interruptsPath: string) =
        let opcodesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opcodes8086.json")
        // Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Interrupt", "dos.json")
        let safePath = if File.Exists(interruptsPath) then Some(interruptsPath) else None
        Decoder.create opcodesPath safePath false

    let touchOperation (state: DecoderState) (bytes: byte[]) =
        Decoder.touchOperation state bytes

    let decode (bytes: byte[]) =
        let state = get("")
        Decoder.decode state bytes
                        |> Decoder.format

    let decodeWith (state: DecoderState) (bytes: byte []) =
        Decoder.decode state bytes
    let decodeRecursive (interruptsPath: string, bytes: byte array, offsets: int array)=
        let decoder = get(interruptsPath)
        let instructions, status, entrySet = Decoder.disassembleRecursive decoder bytes offsets
        Decoder.formatWithLabels instructions bytes status entrySet
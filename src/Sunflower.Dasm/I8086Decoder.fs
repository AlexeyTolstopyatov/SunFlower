namespace Sunflower.Dasm

open System
open System.IO
open Microsoft.FSharp.Linq.RuntimeHelpers

/// <summary>
/// This module represents setup of base intel disassembler for I8086 only!
/// For I186 disassembler use <see cref="I80186Decoder"/>
/// </summary>
module I8086Decoder =
    let get () =
        let opcodesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opcodes8086.json")

        Decoder.create opcodesPath

    let touchOperation (state: DecoderState) (bytes: byte[]) =
        Decoder.touchOperation state bytes
        
    let decode (bytes: byte[]) =
        let state = get()
        Decoder.decode state bytes
                        |> Decoder.format
        
    let decodeWith (state: DecoderState) (bytes: byte []) =
        Decoder.decode state bytes
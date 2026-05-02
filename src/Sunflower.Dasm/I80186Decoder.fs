namespace Sunflower.Dasm

open System
open System.IO

module I80186Decoder =
    let get () =
        let opcodesPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "opcodes80186.json")

        Decoder.create (opcodesPath)
        
    let touchOperation (state: DecoderState) (bytes: byte[]) =
        Decoder.touchOperation state bytes


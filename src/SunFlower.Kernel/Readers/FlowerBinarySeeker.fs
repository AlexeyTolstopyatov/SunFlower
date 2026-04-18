namespace SunFlower.Kernel.Readers

open System
open System.IO

//
// CoffeeLake (C) 2024-2025
// This part of code licensed under MIT
// Check repo documentation
//
// @creator atolstopyatov2017@vk.com
//

/// <summary>
/// Structure of common binary image report
/// </summary>
type FlowerFileInfo =
    {
        /// <summary>
        /// Name of target
        /// </summary>
        Name: string
        /// <summary>
        /// Filesystem path of target
        /// </summary>
        Path: string
        /// <summary>
        /// Size of target
        /// </summary>
        Size: Single
        /// <summary>
        /// Hexadecimal signature view
        /// </summary>
        Sign: string
        /// <summary>
        /// Description of signature
        /// </summary>
        Type: string
    }

/// <summary>
/// Kernel seeker for various binary formats
/// </summary>
module FlowerBinarySeeker =
    type BinaryCheckResult =
        | Found of magic: int * description: string // (magic, description) Tuple
        | NotFound of magic: int
        | Error

    let private readBytes (reader: BinaryReader) position count =
        try
            reader.BaseStream.Seek(position, SeekOrigin.Begin) |> ignore
            Some(reader.ReadBytes(count))
        with _ ->
            None

    let private seekForMz (reader: BinaryReader) =
        match readBytes reader 0L 2 with
        | Some [| 0x4auy; 0x5auy |] -> Found(0x5a4d, "DOS Executable (MZ)")
        | Some [| 0x5auy; 0x4duy |] -> Found(0x4d5a, "DOS Executable (ZM)")
        | Some bytes when bytes.Length = 2 -> NotFound(int bytes[0] ||| (int bytes[1] <<< 8))
        | _ -> Error

    let private seekForNext (reader: BinaryReader) =
        match readBytes reader 0x3CL 4 with
        | Some offsetBytes when offsetBytes.Length = 4 ->
            let offset = BitConverter.ToInt32(offsetBytes, 0)

            match readBytes reader (int64 offset) 2 with
            | Some [| 0x50uy; 0x45uy |] -> Found(0x4550, "WinNT Executable (PE)")
            | Some [| 0x45uy; 0x50uy |] -> Found(0x5045, "WinNT Executable (PE)")
            | Some [| 0x45uy; 0x4euy |] -> Found(0x454e, "Win16-OS/2 1.x Executable (NE)")
            | Some [| 0x4euy; 0x45uy |] -> Found(0x4e45, "Win16-OS/2 1.x Executable (NE)")
            | Some [| 0x45uy; 0x4cuy |] -> Found(0x454c, "Win386-OS/2 2.x Executable (LE)")
            | Some [| 0x4cuy; 0x45uy |] -> Found(0x4c45, "Win386-OS/2 2.x Executable (LE)")
            | Some [| 0x58uy; 0x4cuy |] -> Found(0x584c, "OS/2-ArcaOS Executable (LX)")
            | Some [| 0x4cuy; 0x58uy |] -> Found(0x4c58, "OS/2-ArcaOS Executable (LX)")
            | Some bytes when bytes.Length = 2 -> NotFound(int bytes[0] ||| (int bytes[1] <<< 8))
            | _ -> Error
        | _ -> Error

    let private seekForElf (reader: BinaryReader) =
        match readBytes reader 0L 4 with
        | Some [| 0x7Fuy; 0x45uy; 0x4Cuy; 0x46uy |] -> Found(0x464C457F, "ELF Executable")
        | Some bytes when bytes.Length = 4 -> NotFound(BitConverter.ToInt32(bytes, 0))
        | _ -> Error

    let private seekForIntel (reader: BinaryReader) =
        match readBytes reader 0L 2 with
        | Some [| 0xAAuy; 0x55uy |] -> Found(0xAA55, "Intel ROM")
        | Some [| 0x55uy; 0xAAuy |] -> Found(0x55AA, "Intel ROM")
        | Some bytes when bytes.Length = 2 -> NotFound(int bytes[0] ||| (int bytes[1] <<< 8))
        | _ -> Error

    let private identify (reader: BinaryReader) =
        let checks = [ seekForMz; seekForNext; seekForElf; seekForIntel ]

        checks
        |> List.tryPick (fun check ->
            match check reader with
            | Found(magic, desc) -> Some(magic, desc)
            | _ -> None)
        |> Option.defaultValue (0, "Undefined")

    [<CompiledName "Get">]
    let get (path: string) : FlowerFileInfo =
        use stream = new FileStream(path, FileMode.Open, FileAccess.Read)
        use reader = new BinaryReader(stream)

        let magic, fileType = identify reader
        let fileInfo = FileInfo(path)

        { Name = fileInfo.Name
          Path = path
          Size = float32 fileInfo.Length / 1024f
          Sign = $"0x%X{magic}"
          Type = fileType }

namespace SunFlower.Readers

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
type FlowerBinaryReport = {
    /// <summary>
    /// Name of target
    /// </summary>
    Name : string
    /// <summary>
    /// Filesystem path of target
    /// </summary>
    Path : string
    /// <summary>
    /// Size of target
    /// </summary>
    Size : Int32
    /// <summary>
    /// Hexadecimal signature view
    /// </summary>
    Sign : string
    /// <summary>
    /// Description of signature
    /// </summary>
    Type : string
}
/// <summary>
/// Kernel seeker for various binary formats
/// </summary>
module FlowerBinarySeeker =
    /// <summary>
    /// Checks does image have MZ/ZM ASCII word 
    /// </summary>
    let private hasMarkZbikowskiWord (reader : BinaryReader) =
        0
    /// <summary>
    /// Checks next known ASCII signature
    /// by the e_lfanew pointer
    /// </summary>
    let private hasNextKnownAscii (reader : BinaryReader) =
        0
    /// <summary>
    /// Checks: does image have a a-out valid mid_mag field
    /// </summary>
    let private hasMidMagicWord (reader : BinaryReader) =
        0
    /// <summary>
    /// Checks: does image have a ELF32/+ magic QWORD
    /// </summary>
    /// <param name="reader"></param>
    let private hasElfQWord (reader : BinaryReader) =
        -1
    
    [<CompiledName "Get">]
    let get (path: string) : FlowerBinaryReport =
        let data : FlowerBinaryReport = {
            Name = FileInfo(path).Name
            Size = FileInfo(path).Length |> int
            Path = path
            Sign = "?"
            Type = "Unknown result" 
        }
        
        data
    
namespace SunFlower.Converter

open System
open System.Text
open Avalonia.Data.Converters
open Microsoft.FSharp.Core

module DataConverters =
    /// <summary>
    /// Converts a hexadecimal string to the corresponding text.
    /// Doesn't throw exceptions if the input has odd length.
    /// Throws FormatException if any byte substring is not valid hex.
    /// </summary>
    let asciiBytesToString (hex: string): string =
        match hex with
        | s when String.IsNullOrEmpty(s) -> String.Empty
        | s when s.Length % 2 <> 0 -> "?"
        | s ->
            let asciiSequence =
                Array.init (hex.Length / 2) (fun idx ->
                    let start = idx * 2
                    let pair = hex.Substring(start, 2)
                    // Convert from base 16 & cast the resulting byte
                    let b = Convert.ToByte(pair, 16)
                    char b
                )
            // make a new string from the char array
            String(asciiSequence)
    /// <summary>
    /// Converts string contained unicode bytes into UTF-16 string
    /// Doesn't throw any exceptions if format or argument will be wrong
    /// </summary>
    /// <param name="hex">string with Unicode bytes</param>
    let unicodeBytesToString (hex: string): string =
        match hex with
        | s when String.IsNullOrEmpty(s) -> ""
        | s when s.Length % 4 <> 0 -> "?"
        | s ->
            let unicodeSequence =
                Array.init (hex.Length / 2) (fun idx ->
                    let start = idx * 2
                    let pair = hex.Substring(start, 2)
                    // Convert from base 16 & cast the resulting byte
                    Convert.ToByte(pair, 16)
                )
            // make a new string from the char array
            Encoding.Unicode.GetString(unicodeSequence)
    /// <summary>
    /// Converts (trim) given string into 1-byte integer
    /// Throws ArgumentException if size of given integer
    /// (converted from string) out of range
    /// </summary>
    let UInt8Converter: IValueConverter =
        FuncValueConverter<string, byte>(fun i -> Convert.ToByte i)
    /// <summary>
    /// Converts given string into 2-byte integer 
    /// Throws ArgumentException if size of given integer
    /// (converted from string) out of range
    /// </summary>
    let UInt16Converter: IValueConverter =
        FuncValueConverter<string, uint16>(fun i -> Convert.ToUInt16 i)
    /// <summary>
    /// Converts given string into 4-byte integer
    /// Throws ArgumentException if size of given integer
    /// (converted from string) out of range
    /// </summary>
    let UInt32Converter: IValueConverter =
        FuncValueConverter<string, uint32>(fun i -> Convert.ToUInt32 i)
    /// <summary>
    /// Converts given string into 8-byte integer
    /// Throws ArgumentException if size of given integer
    /// (converted from string) out of range
    /// </summary>
    let UInt64Converter: IValueConverter =
        FuncValueConverter<string, uint64>(fun i -> Convert.ToUInt64 i)
    /// <summary>
    /// Converts given string into sequence of ASCII codes
    /// Doesn't throw any exceptions, because any argument
    /// given by AnyTypeText will be successfully converted to ASCII codes sequcence
    /// </summary>
    let AsciiBytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.ASCII.GetBytes i).Replace('-', ' '))
    /// <summary>
    /// Converts given string into sequence of unicode
    /// Doesn't throw any exceptions
    /// Works same with <c>AsciiBytesConverter</c>: Universal application
    /// </summary>
    let UnicodeBytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.Unicode.GetBytes i).Replace('-', ' '))
    /// <summary>
    /// Converts given string of hexadecimal view of ASCII codes
    /// into CLR string (UTF-16).
    /// Throws ArgumentException!
    /// Returns "?" if current string length
    /// is bad. (ASCII codes must be a pair of characters of given string) 
    /// </summary>
    let AsciiStringConverter: IValueConverter =
        FuncValueConverter<string, string>(asciiBytesToString)
    /// <summary>
    /// Converts given string of hexadecimal view of unicode
    /// into CLR string (UTF-16).
    /// Throws ArgumentException! Returns "?" if current string length
    /// is bad. (Unicode characters must be grouped by 2-pairs) 
    /// </summary>
    let UnicodeStringConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> unicodeBytesToString i)
    
    
namespace SunFlower.Converter

open System
open System.Text
open Avalonia.Data.Converters
open Microsoft.FSharp.Core

module DataConverters =
    /// Convert a hexadecimal string to the corresponding text.
    /// Doesn't throw exceptions if the input has odd length.
    /// Throws FormatException if any byte substring is not valid hex.
    let unicodeBytesToString (hex: string): string =
        match hex with
        | s when String.IsNullOrEmpty(s) -> String.Empty
        | s when s.Length % 2 <> 0 -> "?"
        | s ->
            let charArray =
                Array.init (hex.Length / 2) (fun idx ->
                    let start = idx * 2
                    let pair = hex.Substring(start, 2)
                    // Convert from base 16 & cast the resulting byte
                    let b = Convert.ToByte(pair, 16)
                    char b
                )
            // make a new string from the char array
            charArray |> string
    let asciiBytesToString (hex: string): string =
        match hex with
        | s when String.IsNullOrEmpty(s) -> String.Empty
        | s ->
            
            ""
    let UInt8Converter: IValueConverter =
        FuncValueConverter<string, byte>(fun i -> Convert.ToByte i)
    let UInt16Converter: IValueConverter =
        FuncValueConverter<string, uint16>(fun i -> Convert.ToUInt16 i)
    let UInt32Converter: IValueConverter =
        FuncValueConverter<string, uint32>(fun i -> Convert.ToUInt32 i)
    let UInt64Converter: IValueConverter =
        FuncValueConverter<string, uint64>(fun i -> Convert.ToUInt64 i)
    let AsciiBytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.ASCII.GetBytes i).Replace('-', ' '))
    let UnicodeBytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.Unicode.GetBytes i).Replace('-', ' '))
    let UnicodeStringConverter: IValueConverter =
        FuncValueConverter<string, string>(unicodeBytesToString) 
    let Utf8BytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.UTF8.GetBytes i).Replace('-', ' '))
    let Utf32BytesConverter: IValueConverter =
        FuncValueConverter<string, string>(fun i -> BitConverter.ToString(Encoding.UTF32.GetBytes i).Replace('-', ' '))
    
    
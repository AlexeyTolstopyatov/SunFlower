module SunFlower.Abstractions.Pointers

open System

//
// CoffeeLake (C) 2026-*
// Aliases to default .NET core types for smart formatting
//
// Usage:
// The following struct will be rendered as HTML
//
// [StructLayout(Sequential)]
// public struct RuntimeObject {
//     [MarshalAs(UnmanagedType.U4)] public U32Ptr NamePointer;
//     [MarshalAs(UnmanagedType.U4)] public UInt32 ChildrenCount;
// }
//
// | Name            | Value      |
// |-----------------|------------|
// | NamePointer     | 0xABCDEF00 |
// | ChildrenCount   | 3          |
//
type U8Ptr = Byte
type I8Ptr = Char
type U16Ptr = UInt16
type I16Ptr = Int16
type U32Ptr = UInt32
type I32Ptr = Int32
type U64Ptr = UInt64
type I64Ptr = Int64
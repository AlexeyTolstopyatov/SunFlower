using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;
/// <summary>
/// Основные элементы структуры похоже
/// описываются такой таблицей. И информация
/// берется из ProductID. 
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1, CharSet = CharSet.Ansi)]
public struct RichHeaderEntry
{
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 ProductId; // 0000h
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 ProductBuild; // 0000h
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 Minor;
    [MarshalAs(UnmanagedType.U2)]
    public UInt16 Major; // 0000h
    [MarshalAs(UnmanagedType.U4)]
    public UInt32 Usages; // 00000000h
}

/// <summary>
/// Rich заголовок эво структура добавляемая
/// компиляторами MSVC++. (и скорее всего только ими.)
/// Чтобы эта информация была от Clang/gcc/rustc
/// я ни разу не видел...
///
/// Структура настолько важная, что иногда подвергается
/// удалению через специальное ПО, чтобы затруднить анализ
/// используемых инструментов и структуры программы.
/// В спецификации Microsoft её нигде нет, соответсвенно
/// полагаю, что она опциональна. (возможно как раз факт о MSVC)
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct RichHeader()
{
    [MarshalAs(UnmanagedType.I4)]
    public UInt32 DansMagic = 0;
    [MarshalAs(UnmanagedType.I4)]
    public UInt32 XorKey = 0;
    [MarshalAs(UnmanagedType.I4)]
    public UInt32 Magic = 0;
    [MarshalAs(UnmanagedType.I4)]
    public UInt32 Checksum = 0;
    [MarshalAs(UnmanagedType.LPArray)]
    public RichHeaderEntry[] Entries = [];
}
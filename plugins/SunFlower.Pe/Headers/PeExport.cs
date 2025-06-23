using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct PeImageExportDirectory
{
    public UInt32 Characteristics;
    public UInt32 TimeDateStamp;
    public UInt16 MajorVersion;
    public UInt16 MinorVersion;
    public UInt32 Name;               // RVA имени модуля
    public UInt32 Base;               // Базовый ординал
    public UInt32 NumberOfFunctions;  // Количество экспортируемых функций
    public UInt32 NumberOfNames;      // Количество именованных функций
    public UInt32 AddressOfFunctions; // RVA массива адресов функций
    public UInt32 AddressOfNames;     // RVA массива имен функций
    public UInt32 AddressOfNameOrdinals; // RVA массива ординалов
}

public class ExportFunction
{
    public String Name { get; set; } = String.Empty;
    public UInt32 Ordinal { get; set; }
    public UInt64 Address { get; set; }
}
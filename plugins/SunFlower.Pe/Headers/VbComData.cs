using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;
/// <summary>
/// Ведущая структура которая включает в себя
/// опционально таблицы <see cref="VbComRegistrationData"/>,
/// <see cref="VbComRegistrationInfo"/>, <see cref="VbDesignerInfo"/>
/// ~ЕСЛИ~ изучаемый бинарник представляет из себя ActiveX объект.
///
/// Боже мой, дай мне сил разобраться до логического конца
/// в этом Microsoft наследии.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VbComData
{
    [MarshalAs(UnmanagedType.Struct)] public VbComRegistration VbComRegistrationData;
    [MarshalAs(UnmanagedType.Struct)] public VbComRegistrationInfo VbComRegistrationInfo;
    [MarshalAs(UnmanagedType.Struct)] public VbDesignerInfo VbDesignerInfo;
}

// COM data -> {REG-Data, REG-INFO, Designer-INFO}
/// <summary>
/// Содержит информацию используемую если собранный
/// двоичный файл является ActiveX и содержит значимые COM
/// данные для реестра (COM registration data может переводиться по-разному в контексте Windows)
/// такие как: информация о библиотеке типов данных/структур, уникальный номер
/// зарегистрированного объекта (CLSID) и данные Designer 'a (вообще не воображу что под дизайнером имеют ввиду)
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct VbComRegistration
{
    [FieldOffset(0x00)] public Byte RegInfoOffset; // offset to COM interface information
    [FieldOffset(0x04)] public UInt16 ProjectNameOffset;
    [FieldOffset(0x08)] public UInt16 HelpDirectoryOffset;
    [FieldOffset(0x0C)] public UInt16 ProjectDescriptionOffset;
    [FieldOffset(0x10)] public UInt64 UuidProjectClsId;
    [FieldOffset(0x20)] public UInt32 TypeLibraryLanguageId;
    [FieldOffset(0x24)] public UInt16 Unknown;
    [FieldOffset(0x26)] public UInt16 TypeLibraryMajor;
    [FieldOffset(0x28)] public UInt16 TypeLibraryMinor;
}
/// <summary>
/// Если необходимо зарегистрировать действительный объект,
/// то RegData->RegInfoOffset <see cref="VbComRegistration"/> укажет
/// на следующую структуру для каждого действительного объекта:
/// </summary>
[StructLayout(LayoutKind.Explicit)]
public struct VbComRegistrationInfo
{
    [FieldOffset(0x00)] public Byte NextObjectOffset; // COM interfaces offset
    [FieldOffset(0x04)] public Byte ObjectNameOffset; // offset to obj-name
    [FieldOffset(0x08)] public Byte ObjectDescriptionOffset;
    [FieldOffset(0x0C)] public UInt32 InstancingMode;
    [FieldOffset(0x10)] public UInt32 ObjectId; // ID of current object
    [FieldOffset(0x14)] public UInt64 UuidObject; // Class ID (CLSID) of object
    [FieldOffset(0x24)] public UInt32 IsInterface; // If next CLSID is valid
    [FieldOffset(0x28)] public UInt32 UuidObjectInterfaceOffset;
    [FieldOffset(0x2C)] public UInt32 UuidEventsInterfaceOffset;
    [FieldOffset(0x30)] public UInt32 HasEvents; // specifies if CLSID above is valid
    [FieldOffset(0x34)] public UInt32 MiscStatus; // OLE misc flags storage
    [FieldOffset(0x38)] public Byte ClassType;
    [FieldOffset(0x39)] public Byte ObjectType;
    [FieldOffset(0x3A)] public UInt16 ToolBoxBitmap32; // Control bitmap ID in Toolbox
    [FieldOffset(0x3C)] public UInt16 DefaultIcon;
    [FieldOffset(0x3E)] public UInt16 IsDesigner; // specifies if this obj = designer
    [FieldOffset(0x40)] public UInt16 DesignerDataOffset;
}

/// <summary>
/// Designer info содержит ~Binary Strings~,
/// которые к моему счастью используются в областях COM
/// и в C# тоже. Так что полагаю, String здесь прокатит.
/// </summary>
public struct VbDesignerInfo
{
    [MarshalAs(UnmanagedType.BStr)] public String AddInRegKey;
    [MarshalAs(UnmanagedType.BStr)] public String AddInName;
    [MarshalAs(UnmanagedType.BStr)] public String AddInDescription; 
    public UInt32 LoadBehaviour; 
    [MarshalAs(UnmanagedType.BStr)] public String SatelliteDll; 
    [MarshalAs(UnmanagedType.BStr)] public String AdditionalRegKey;
    public UInt32 CommandLineSafe; // 0 - GUI <-> 1 - GUI-less
}
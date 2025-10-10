using System.Runtime.InteropServices;

namespace SunFlower.Pe.Headers;
[StructLayout(LayoutKind.Sequential)]
public struct VbComRegistration
{
    public UInt32 RegInfoOffset; // offset to COM interface information
    public UInt32 ProjectNameOffset;
    public UInt32 HelpDirectoryOffset;
    public UInt32 ProjectDescriptionOffset;
    public UInt64 UuidProjectClsId;
    public UInt32 TypeLibraryLanguageId;
    public UInt32 Unknown;
    public UInt16 TypeLibraryMajor;
    public UInt16 TypeLibraryMinor;
}
[StructLayout(LayoutKind.Sequential)]
public struct VbComRegistrationInfo
{
    public UInt32 NextObjectOffset; // COM interfaces offset
    public UInt32 ObjectNameOffset; // offset to obj-name
    public UInt32 ObjectDescriptionOffset;
    public UInt32 InstancingMode;
    public UInt32 ObjectId; // ID of current object
    public UInt64 UuidObject; // Class ID (CLSID) of object
    public UInt32 IsInterface; // If next CLSID is valid
    public UInt32 UuidObjectInterfaceOffset;
    public UInt32 UuidEventsInterfaceOffset;
    public UInt32 HasEvents; // specifies if CLSID above is valid
    public UInt32 MiscStatus; // OLE misc flags storage
    public Byte ClassType;
    public Byte ObjectType;
    public UInt16 ToolBoxBitmap32; // Control bitmap ID in Toolbox
    public UInt16 DefaultIcon;
    public UInt16 IsDesigner; // specifies if this obj = designer
    public UInt32 DesignerDataOffset;
}
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
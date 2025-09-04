using SunFlower.Le.Headers.Le;

namespace SunFlower.Le.Services;

public class VxdDriverManager : UnsafeManager
{
    public VxdHeader DriverHeader { get; set; }
    public VxdResources DriverResources { get; set; }
    public VxdDescriptionBlock DriverDescriptionBlock { get; set; }

    /// <summary>
    /// Offset appends to current reader position. 
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="offset"></param>
    public VxdDriverManager(BinaryReader reader, uint offset)
    {
        reader.BaseStream.Position += offset;
        DriverHeader = Fill<VxdHeader>(reader);
        
        if (DriverHeader.LE_WindowsResOffset == 0)
            return;

        reader.BaseStream.Position = DriverHeader.LE_WindowsResOffset;
        DriverResources = Fill<VxdResources>(reader);
        // version Win32 resources goes here
        // DDB block requires more information.
    }
}
using System.Data;
using System.Diagnostics;
using SunFlower.Abstractions;

namespace SunFlower.Pe;

/// <summary>
/// Template of IFlowerSeed implementation.
/// </summary>
public class PortableExecutableSeed : IFlowerSeed
{
    public string Seed => "PE IMAGE Windows NT x86-64";
    public FlowerSeedStatus Status { get; set; } = new FlowerSeedStatus();

    /// <summary>
    /// Second important functional
    /// Finds and fills <see cref="Status"/>
    /// </summary>
    /// <param name="path"></param>
    public void Analyse(string path)
    {
        try
        {
            Status.IsEnabled = true;
            
            using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read);
            using BinaryReader reader = new BinaryReader(stream);
            
            stream.Position = 0x3C;
            uint peHeader = reader.ReadUInt32();

            stream.Position = peHeader; // move by e_lfanew pointer
            uint head = reader.ReadUInt32();
            
            if (head != 0x00004550) // "PE\0\0" little-endian
            {
                Status.IsEnabled = false;
                
                return;
            }

            DataTable metaTable = new("PEMetadata");
            metaTable.Columns.Add("Field", typeof(string));
            metaTable.Columns.Add("Value", typeof(string));
            
            metaTable.Rows.Add("Signature", "PE");
            metaTable.Rows.Add("HeaderOffset", peHeader.ToString("X"));
            
            //
            stream.Position = peHeader + 4;
            ushort machineType = reader.ReadUInt16();
            metaTable.Rows.Add("Machine", $"0x{machineType:X4}");

            Status.Result = new[] { metaTable };
            Status.IsEnabled = true;
        }
        catch (Exception ex)
        {
            Debug.Write($"Plugin: {ex}");
            Status.IsEnabled = false;
        }
    }
    
    /// <summary>
    /// EntryPoint returns Status Table 
    /// </summary>
    /// <returns></returns>
    public int Main(string path) // <-- path to needed image 
    {
        try
        {
            Analyse(path);
            return Status.IsEnabled ? 0 : 1;
        }
        catch
        {
            return -1;
        }
    }
}
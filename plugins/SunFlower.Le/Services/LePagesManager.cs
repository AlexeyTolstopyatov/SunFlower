using SunFlower.Le.Headers.Le;
using SunFlower.Le.Models.Le;

namespace SunFlower.Le.Services;

public class LePagesManager
{
    public List<ObjectPageModel> Pages { get; set; } = [];

    public LePagesManager(BinaryReader reader, uint offset, uint count)
    {
        reader.BaseStream.Position = offset;
        
        for (var i = 0; i < count; i++)
        {
            var entry = new ObjectPage
            {
                HighPage = reader.ReadUInt16(),
                LowPage = reader.ReadByte(),
                Flags = reader.ReadByte()
            };
            // alignment skip
            reader.ReadUInt32(); 
            
            ToModel(entry);
        }
    }

    private void ToModel(ObjectPage page)
    {
        List<string> flags = [];
        
        switch (page.Flags & (byte)ObjectPage.PageFlags.TypeMask)
        {
            case (byte)ObjectPage.PageFlags.Legal:
                flags.Add("LEGAL");
                break;
            case (byte)ObjectPage.PageFlags.Iterated:
                flags.Add("ITERATED");
                break;
            case (byte)ObjectPage.PageFlags.Invalid:
                flags.Add("INVALID");
                break;
            case (byte)ObjectPage.PageFlags.ZeroFilled:
                flags.Add("BSS");
                break;
        }
    
        if ((page.Flags & (byte)ObjectPage.PageFlags.LastPageInFile) != 0)
        {
            flags.Add("LAST");
        }
        
        Pages.Add(new ObjectPageModel(page, flags));
    }
}
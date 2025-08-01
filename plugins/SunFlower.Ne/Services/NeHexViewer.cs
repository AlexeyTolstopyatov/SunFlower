using System.Text;

namespace SunFlower.Ne.Services;

public class NeHexViewer
{
    /// <summary>
    /// Makes string of hexadecimal view of current segment
    /// </summary>
    /// <param name="bytes">current segment</param>
    /// <param name="title">title of this view</param>
    /// <param name="position">stream position at the moment</param>
    /// <returns></returns>
    public string Make(byte[] bytes, string title, long position)
    {
        StringBuilder sb = new();
        sb.AppendLine($"### {title}");
        
        
        
        return sb.ToString();
    }
}
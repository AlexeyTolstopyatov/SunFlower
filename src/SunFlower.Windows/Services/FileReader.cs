using System.IO;

namespace SunFlower.Windows.Services;

public class FileReader : IDisposable
{
    public FileStream Stream { get; private set; }

    public BinaryReader Reader { get; private set; }
    public void InitializeStream(string path)
    {
        try
        {
            Stream = new(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            Reader = new(Stream);
        }
        catch(Exception e)
        {
            // ignore
            Console.WriteLine(e);
        }
    }

    public void Dispose()
    {
        Stream.Dispose();
        Reader.Dispose();
    }
}
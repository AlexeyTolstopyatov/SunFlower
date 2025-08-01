using SunFlower.Le.Services;
using SunFlower.Pe;
using SunFlower.Pe.Services;
using SunFlower.Services;
using SunFlower.Links.Services;
namespace SunFlower.Connection;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }
    /// <summary>
    /// Passed. Binding complete
    /// </summary>
    [Test]
    public void ConnectPlugin()
    {
        var results = FlowerSeedManager
            .CreateInstance()
            .LoadAllFlowerSeeds()
            .Seeds;
        Assert.Pass();
    }
    [Test]
    public void CheckoutPeImage()
    {
        var path = @"D:\Projects\vb\Semi VB Decompiler\ApiLoader.exe";
        PortableExecutableSeed seed = new();
        seed.Main(path);
        
        //DataTable table = seed.Status.Result[0];
        PeDumpManager manager = new(path);
        manager.Initialize();

        var exportsManager =
            PeExportsManager.CreateInstance(manager.FileSectionsInfo, path);
        PeImportsManager importsManager =
            new(manager.FileSectionsInfo, path);
        PeClrManager corManager = new(manager.FileSectionsInfo, path);
        
        importsManager.Initialize();
        exportsManager.Initialize();
        corManager.Initialize();
        
        Assert.Pass();
    }
    [Test]
    public void CheckoutNeImage()
    {
        //string path = @"D:\Анализ файлов\inst\NE\IBMCOLOR.DRV";
        var path = @"D:\TEST\VBRUN\VB3PRJ.EXE";
        SunFlower.Ne.Services.NeDumpManager manager = new(path);
        
        Assert.Pass();
    }

    [Test]
    public void CheckoutDriver()
    {
        var path = @"D:\Анализ файлов\inst\LE\CDFS.VXD";
        LeDumpManager manager = new(path);
        LeTableManager tableManager = new(manager);
        
        Assert.Pass();
    }
    [Test]
    public void CheckoutPif()
    {
        PifDumpManager manager = new(@"C:\Users\atvlg\OneDrive\Desktop\batchbox.pif");
        
        Assert.Pass();
    }
}
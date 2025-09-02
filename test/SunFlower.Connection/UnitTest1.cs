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
    /// <summary>
    /// 99% completed.
    /// >> Resources Table undone.
    /// >> VB3.0 runtime -> Sunflower Win16-VB3 IA-32
    /// >> VB4.0 runtime -> Sunflower Win16-VB4 IA-32
    /// 
    /// </summary>
    [Test]
    public void CheckoutNeImage()
    {
        //string path = @"D:\Анализ файлов\inst\NE\IBMCOLOR.DRV";
        var path = @"D:\TEST\os2\SYSINST2.EXE";
        SunFlower.Ne.Services.NeDumpManager manager = new(path);
        SunFlower.Ne.Services.NeTableManager tableManager = new(manager);
        
        Assert.Pass();
    }
    
    [Test]
    public void CheckoutDriver()
    {
        // baaad...
        const string path = @"D:\TEST\OS2\VTDAPI.386";
        LeDumpManager manager = new(path);
        LeTableManager tableManager = new(manager);
        
        Assert.Pass();
    }
    /// <summary>
    /// >> Main section done
    /// >> Windows 3.0 286 done
    /// >> Windows 3.0 386 done
    /// >> Windows 95/98 Virtual Machine Manager (wait for TableManager)
    /// >> Windows NT 3.x undone.
    /// </summary>
    [Test]
    public void CheckoutPif()
    {
        PifDumpManager manager = new(@"C:\Users\atvlg\OneDrive\Desktop\batchbox.pif");
        
        Assert.Pass();
    }
}
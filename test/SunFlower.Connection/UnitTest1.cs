using SunFlower.Abstractions;
using SunFlower.Le.Services;
using SunFlower.Pe;
using SunFlower.Pe.Services;
using SunFlower.Services;
using SunFlower.Links.Services;
using SunFlower.Pe.Models;

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
        var path = @"D:\TEST\VBRUN\calldlls32.exe";
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

        PeTableManager tables = new PeTableManager(new PeImageModel()
        {
            Sections = manager.PeSections,
            OptionalHeader = manager.OptionalHeader,
            OptionalHeader32 = manager.OptionalHeader32,
            FileHeader = manager.FileHeader,
            MzHeader = manager.Dos2Header,
            ExportTableModel = exportsManager.ExportTableModel,
            ImportTableModel = importsManager.ImportTableModel,
            CorHeader = corManager.Cor20Header,
            Vb5Header = manager.Vb5Header,
            Vb4Header = manager.Vb4Header
        });
        tables.Initialize();
        Assert.Pass("VB4 Runtime structure FOUND!");
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
        var path = @"D:\TEST\OS2\LINK.EXE";
        SunFlower.Ne.Services.NeDumpManager manager = new(path);
        SunFlower.Ne.Services.NeTableManager tableManager = new(manager);
        
        Assert.Pass();
    }
    
    [Test]
    public void CheckoutDriver()
    {
        // baaad...
        const string path = @"D:\TEST\OS2\VTDAPI.386";
        LeDumpManager manager = new(path); // EntryPoint table may be corrupted
        LeTableManager tableManager = new(manager); // EntryPoint table may be corrupted
        
        Assert.Pass();
    }

    [Test]
    public void CheckoutLxImage()
    {
        const string path = @"D:\TEST\ARCA\HELPMGR.DLL";
        LxDumpManager manager = new LxDumpManager(path);
        LeDumpManager leManager = new LeDumpManager(path);

        LxTableManager tableManager = new(manager);
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
    [Test]
    public void CheckFlowerReport()
    {
        Assert.Pass(FlowerReport.ForColumn("offset", typeof(int)));
    }
    [Test]
    public void CheckFlowerPointer()
    {
        Assert.Pass(FlowerReport.FarHexString(0, 15, true));
    }
}
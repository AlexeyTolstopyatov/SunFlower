using SunFlower.Abstractions;
using SunFlower.Le.Services;
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
    public void FindVb5()
    {
        var path = @"D:\VB3TOOLS\VBDIS3.67e_Reloaded_Rev3_DoDi_s_VB3Decompiler\VBDIS3.67e\FRMS2TXT.exe";

        var peManager = new PeDumpManager(path);
        peManager.Initialize();
        var sectionsInfo = new FileSectionsInfo
        {
            BaseOfCode = peManager.OptionalHeader32.BaseOfCode,
            ImageBase = peManager.OptionalHeader32.ImageBase,
            BaseOfData = peManager.OptionalHeader32.BaseOfData,
            EntryPoint = peManager.OptionalHeader32.AddressOfEntryPoint,
            FileAlignment = peManager.OptionalHeader32.FileAlignment,
            Is64Bit = false,
            NumberOfRva = peManager.OptionalHeader32.NumberOfRvaAndSizes,
            NumberOfSections = peManager.FileHeader.NumberOfSections,
            Sections = peManager.PeSections,
            Directories = peManager.PeDirectories,
            SectionAlignment = peManager.OptionalHeader32.SectionAlignment
        };
        var manager = new Vb5ProjectTablesManager(path, peManager.VbOffset, peManager.Vb5Header, sectionsInfo);
        
        Assert.Pass(manager.ProjectName);
    }
    [Test]
    public void CheckoutPeImage()
    {
        var path = @"C:\Program Files\Oracle\VirtualBox\VirtualBoxVM.exe";
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

        PeImageView tables = new PeImageView(new PeImageModel()
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
            Vb4Header = manager.Vb4Header,
            Directories = manager.PeDirectories
        });
        tables.Initialize();
        Assert.Pass("no null references");
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
using System.Data;
using SunFlower.Pe;
using SunFlower.Pe.Services;
using SunFlower.Services;

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
        FlowerSeedManager bridge = new();
        bridge.LoadAllFlowerSeeds();
        bridge.GetAllInvokedSeedResults(@"D:\Анализ файлов\inst\PE\acpi.sys");
        Assert.Pass();
    }
    [Test]
    public void CheckoutPeImage()
    {
        PortableExecutableSeed seed = new();
        seed.Analyse(@"D:\Анализ файлов\inst\PE\acpi.sys");
        
        //DataTable table = seed.Status.Result[0];
        PortableExecutableDumpManager manager = new(@"D:\Анализ файлов\inst\PE\acpi.sys");
        manager.Initialize();

        PortableExecutableSectionDumpManager sectionDumpManager =
            PortableExecutableSectionDumpManager.CreateInstance(manager.FileSectionsInfo, @"D:\Анализ файлов\inst\PE\acpi.sys");
        sectionDumpManager.Initialize();
        
        Assert.Pass();
    }
}
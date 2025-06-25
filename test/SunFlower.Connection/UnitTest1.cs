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
        var results = FlowerSeedManager
            .CreateInstance()
            .LoadAllFlowerSeeds()
            .UnloadUnusedSeeds()
            .Seeds;
        Assert.Pass();
    }
    [Test]
    public void CheckoutPeImage()
    {
        PortableExecutableSeed seed = new();
        seed.Main(@"D:\Анализ файлов\inst\PE\acpi.sys");
        
        //DataTable table = seed.Status.Result[0];
        PortableExecutableDumpManager manager = new(@"D:\Анализ файлов\inst\PE\acpi.sys");
        manager.Initialize();

        PortableExecutableExportsManager exportsManager =
            PortableExecutableExportsManager.CreateInstance(manager.FileSectionsInfo, @"D:\Анализ файлов\inst\PE\acpi.sys");
        exportsManager.Initialize();
        
        Assert.Pass();
    }
}
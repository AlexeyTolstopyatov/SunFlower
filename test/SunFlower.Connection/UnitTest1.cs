using System.Data;
using SunFlower.Pe;
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
        FlowerSeedConnect bridge = new();
        bridge.Initialize();
        bridge.GetEntryPointTables(@"D:\Анализ файлов\inst\PE\acpi.sys");
        Assert.Pass();
    }
    [Test]
    public void CheckoutPeImage()
    {
        PortableExecutableSeed seed = new();
        seed.Analyse(@"D:\Анализ файлов\inst\PE\acpi.sys");
        
        DataTable table = seed.Result.Result[0];
        
        Assert.Pass();
    }
}
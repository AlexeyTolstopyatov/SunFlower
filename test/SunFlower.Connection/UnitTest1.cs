using SunFlower.Abstractions;
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
            .Seeds;
        
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
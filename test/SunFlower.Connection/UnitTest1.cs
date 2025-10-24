using System.Data;
using SunFlower.Abstractions;
using SunFlower.Services;

namespace SunFlower.Connection;

public struct TestableStruct
{
    public ushort MajorVersion;
    public ushort MinorVersion;
    public byte StructType;
    public bool IsUnused;
    public int TotalObjects;
    public string Name;
}

public class TestableClass
{
    public uint DataType { get; } = 0;
    public string Name { get; set; } = "CLASS_NAME";

    public UInt64 NotPointer = 0x7FFA;
}

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void TestTables()
    {
        var test = new TestableStruct
        {
            MajorVersion = 0x0001,
            MinorVersion = 0x0000,
            StructType = 0xFF,
            IsUnused = true,
            Name = "STRUCT_NAME"
        };
        //var testc = new TestableClass();
        //var dt = FlowerReflection.GetNameValueTable(testc);
        //Terminal.DataView.print_table(dt);
        List<TestableClass> list = [new TestableClass(), new TestableClass()];

        var dts = FlowerReflection.ListToDataTable(list);
        
        Assert.Pass();
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
using System.Diagnostics;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using NUnit.Framework.Internal;
using SunFlower.Ne.Headers;
using SunFlower.Ne.Models;
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
            .Seeds;
        Assert.Pass();
    }
    [Test]
    public void CheckoutPeImage()
    {
        string path = @"D:\Projects\vb\Semi VB Decompiler\ApiLoader.exe";
        PortableExecutableSeed seed = new();
        seed.Main(path);
        
        //DataTable table = seed.Status.Result[0];
        PeDumpManager manager = new(path);
        manager.Initialize();

        PeExportsManager exportsManager =
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
        string path = @"D:\Projects\WINFO\VB3PRJ.EXE";
        SunFlower.Ne.Services.NeDumpManager manager = new(path);
        
        Assert.Pass();
    }

    [Test]
    public void CheckoutDriver()
    {
        string path = @"D:\Анализ файлов\inst\LE\CDFS.VXD";
        SunFlower.Le.Services.LeDumpManager manager = new(path);
        
        Assert.Pass();
    }
}
# Plugins development | usage of `SunFlower.Abstractions`

<img src="assets/sunflower.svg" align="right" width="128" height="128">

All plugins what connects with SunFlower client are uses basic definitions
what declared in the `SunFlower.Abstractions` namespace

### First stage | Plugin-side code

You already made project ("Class Library" solution) and added 
reference to `SunFlower.Abstractions.dll`.
Main source file in the solution is a "MySeed.cs"

Let's find something in file. Imagine, we have secret string in the
"secrets.bin"

We want to save results and send it to the client.

```cs
using SunFlower.Abstractions;

namespace MySeedLibrary;

public class MySeed : IFlowerSeed {
    /// Title
    public string Name => "It shows in Connected Plugins menu";
    /// Plugin results writes here. All exception chains
    /// contains here. When exception throws -> 
    /// plugin terminates and information shows in a Client app.
    public FlowerSeedStatus Status { get; set; }
    /// EntryPoint 
    /// (calls when IFlowerSeed derivate instance creates)
    public int Main(string path) { 
        // In example we want to find secret string in binary
        try
        {
            using var stream = new FileStream(path, ...);
            using var reader = new BinaryReader(stream);

            var stringOffset = reader.ReadUInt32();
            stream.Position = stringOffset;

            var secrect = Encoding.ASCII.GetString(reader.ReadBytes(8));
            // Fine. String defined
            // Saving results
            var result = new FlowerSeedResult(FlowerSeedEntryType.String, secret);
            Status.Results.Add(result);

            Statis.IsEnabled = true; // manually set flag we can use it
            return 0; // <-- save changes & exit
        }
        catch(Exception e)
        {
            // Remember the problem what's happened
            Status.LastError = e;
            return -1; // <-- send bad signal
        }
    }
}
```

### Results Decorators | `FlowerReport` and `FlowerReflection`

For better view of given result, you can use `FlowerReport` module.
 - `FlowerReport.SafeString(string)` - Shield .NET string
 - `FlowerReport.ForColumn(string, Type)` - Column name with shorten type
 - `FlowerReport.ForColumnFl(string, FlowerType)` - Column name with shorten type
 - `FlowerReport.ForColumnStr(string, string)` - Column name with custom string-defined type
 - `FlowerReport.FarHexString(byte, byte, bool)` - Traditional view of FAR pointer
 - `FlowerReport.Far32HexString(byte, uint16, bool)` - Traditional view of FAR32 pointer

If you want to represent array or list of something, `FlowerReflection`
module helps with the same taska  
 - `FlowerReflection.GetNameValueTable<T>(T)` - `struct/class` into DataTable instance
 - `FlowerReflection.ListToDataTable<T>(IEnumerable<T>)` - List of one typed instances into DataTable

### Plugin metadata | `[Flower]` and `[FlowerContract]` bindings

Before you try to run SunFlower with your new plugin
you need to know little about communication.
You must define plugin version same with what platform you're using

```cs
using SunFlower.Abstractions;

namespace MySeedLibrary;

[FlowerContract(5, 0, 0)] // With which plugins manager we works
[Flower(SeedTarget.Data)] // With which file scope we works 
public class MySeed : IFlowerSeed {/* ... */}
```

After this you can move built assembly into `/Plugins`
and run SunFlower client for a current file. 
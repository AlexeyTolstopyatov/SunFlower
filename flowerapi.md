# Plugins development | usage of `SunFlower.Abstractions` 5.0

<img src="assets/sunflower.svg" align="right" width="128" height="128">

### Abstract

Since the great Sunflower 5.0 release the plugin model contains breaking changes.
While the release was being tested and used for various purposes, 
the concept of a user workspace was rethought and rebuilt entirely from scratch. 
It is a significant, experiment to position this alongside the workspace paradigms 
found in such industry titans as IDA Pro or Ghidra.

For now the concept of user workspace mostly focuses on file/process model layout.

## Getting started

Same with all .NET asssemblies and projects, all plugins what connects 
with SunFlower client are uses basic definitions
what declared in the `SunFlower.Abstractions` namespace.

I'm counting that you've made a project ("Class Library" solution) already and added 
reference to `SunFlower.Abstractions.dll`.

1. Create a class called with a "Flower" postfix and implement an `IFlower` plugin interface 
2. Define a calling convention (set `[FlowerContract]` version attribute with 5 major version)

### Client Presentation Modes

Depending on what you want to show, the sunflower client will generate markdown book
or just makes a single file with raw output, e.g. assembler pseudo code output
The breaking concept of sunflower was a present a formatted report or a little book
about researching program, not only a program text.

That's why the `[Flower]` attribute tells which mode will used after 
main plugin function was executed.

### New plugin code listing

Now here it is a simple code listing 

```csharp
using SunFlower.Abstractions;
using System.Threading.Tasks;

[Flower(ResultType.Data)]
[FlowerContract(5, 0, 0)]
public class MyFirstGrownFlower : IFlower {
    
    [Seed(
        name: "File size",
        description: "..."
    )]
    public string InitialMessage { get; }

    // IFlower members implementation starts 
    public Task Main(string path) => Task.Run(() => {
        InitialMessage = $"{new FileInfo(path).Length / 0x400}KiB";
    });
    public string Name => "It shows in command palette/menu";
    // IFlower members implementation ends
} 
```

This should be fairly straightforward to understand. 
All fields and properties marked with `[Seed]` will be captured and processed by the loader.

## Results Decorators | `FlowerReport` and `FlowerReflection`

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

### Sunflower
Sunflower is an open-source, plugin-driven system designed for binary analysis. Was inspired by Ghidra’s internals.
This is a extensible Binary Analysis and Recognition Framework

All core plugins moved from JellyBins

### Sunflower seeds (application plugins)

1) Create Visual Studio solution.
2) Add reference `SunFlower.Abstractions.dll`
3) Make sure: no differences between Client app version and Abstractions
4) Follow this template

```csharp
public class MyAnalyzer : IFlowerSeed {
  /// Title
  public string Name => "It shows in Connected Plugins menu";
  /// Plugin results writes here. All exception chains
  /// contains here. When exception throws -> 
  /// plugin terminates and information shows in a Client app.
  public FlowerSeedStatus Status { get; set; }
  /// EntryPoint 
  /// (calls when IFlowerSeed derivate instance creates)
  public int Main(string path) { /* Scan for patterns */ }
}
```

If you want use F# toolchain you can implement it like this:

```fsharp
type MyAnalyzer() =
  interface IFlowerSeed with
  /// Title
  member this.Name = "It shows in Connected Plugins menu"
  /// Plugin results writes here. All exception chains
  /// contains here. When exception throws -> 
  /// plugin terminates and information shows in a Client app.
  member val Status = FlowerSeedStatus() with get, set
  /// EntryPoint
  /// (calls when IFlowerSeed derivate instance creates)
  member this.Main(path: string) : int = 
    // Scan for patterns
    0
```

5) Build and Drop .DLL into `%Application%/Plugins`

### An architecture problems that seriously bother me, but I can't fix them

 - Stupid Exceptions handling -
A `Main` procedure contains exceptions handler which
rewrites Status last error field. Loader prints this message with `--- Begin/End Exception chain ---` brakets.
 - Versions incompatibility - Unfortunately Sunflower plugins which are differ the foundation are **incompatible** at the moment of updating documentation.
Any differences between foundation file version and plugins foundation calls force exit (means conflict behaviour).

### License
MIT License

Copyright (c) 2024-2025 CoffeeLake

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

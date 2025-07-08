### Sunflower
Sunflower is an open-source, plugin-driven system designed for binary analysis. Was inspired by Ghidra, PEAnathomist, CFFExplrer, VBSemiDecompiler, and other toolkits. This is a extensible Binary Analysis idea.

This repository includes 4 parts of my work:
 - Extensions Loader (F# `.net8.0`)
 - Core Plugins (moved JellyBins parts)
 - Terminal Client (F# `.net8.0`)
 - [Windowed Client](WINDOWS.md) (C# `.net8.0` / JavaScript)
    - Native part (`PINVOKE`/`FFI` usages/`Win32 API` base/...)
    - Monaco Editor bindings

All core plugins moved from JellyBins ~(JellyBins obsolete)~

Main idea was an isolation of add-ons because main codebase had become
very large. The previous project was rewritten from scratch five times, and in an undone state of parts is contained here. 

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

 - **Stupid** Exceptions handling -
A `Main` procedure contains exceptions handler which
rewrites Status last error field. Loader prints this message with `--- Begin/End Exception chain ---` brakets.
 - Versions incompatibility - Unfortunately Sunflower plugins which are differ the foundation are **incompatible** at the moment of updating documentation.
Any differences between foundation file version and plugins foundation calls force exit (means conflict behaviour).

### Frameworks And other external toolchain

Despite the fact that the loader's core uses only the capabilities of `.NET` Core platform, and bundled with the loader's main plugins are written from scratch without the use of external tools, the window application `Sunflower.Windows.exe` uses many different add-ons to be more comfortable and modern. 

All frameworks and toolkits
 - WPF `.net-windows7.0`
 - `.NET 8.0` - Foundation of Windowed client
 - [HandyControls](https://github.com/HandyOrg/HandyControl) `3.4.0` - better Window controls / little MVVM experience
 - [Microsoft WPF Behaviours](https://github.com/microsoft/XamlBehaviorsWpf) - The MVVM experience
 - [Monaco](https://github.com/microsoft/monaco-editor) `0.52` - All flower-extension results in one document 
 - [Monaco-Markdown](https://github.com/trofimander/monaco-markdown) - Highlighting extension for Markdown documents
 - Win32 bindings - `OpenFileDialog` / `SaveFileDialog` bindings
 - [Microsoft Web View](https://github.com/MicrosoftEdge/WebView2Browser) - Toolkit for Monaco editor support.

### Beeing honest

This is my largest work at the moment of publishing readme.
Many places have incorrect patterns or ideas, and modules have strange code-scopes. But I've done the part of it and next time this toolkit will be better /I hope/. 

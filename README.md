### Sunflower

<img src="assets/sunflower.svg" height="128" width="128" align="right">

Sunflower is an open-source, plugin-driven system designed for binary analysis. 
Was inspired by Ghidra, PEAnathomist, CFFExplrer, Semi VB Decompiler, and other toolkits. 
Main idea of it -- make non-monolith application and avoid embedded functions. This repository contains
just loader details and Windows client.   

This repository includes 4 parts of my work:
 - Extensions Loader (F# `.net8.0`)
 - Core Plugins (moved JellyBins parts)
 - Terminal Client (F# `.net8.0`)
 - [Windowed Client](WINDOWS.md) (C# `.net8.0` / JavaScript)
    - Native part (`PINVOKE`/`FFI` usages/`Win32 API` base/...)
    - Monaco Editor bindings

All core plugins moved from [JellyBins](https://github.com/AlexeyTolstopyatov/jellybins) ~(JellyBins obsolete)~

Main idea was an isolation of add-ons because main codebase had become
very large. The previous project was rewritten from scratch five times, and in an undone state of parts is contained here. 

### Sunflower client

User guide for client stores [here](WINDOWS.md)

<img src="assets/mdbook.png">

### Sunflower core "seeds" (plugins)

In the package stores moved from JellyBins parts of code
for definition the
 - `MZ` Executables (real-mode x86 applications);
 - `NE` segmented Executables (protected-mode x86 applications);
 - `LE` OS/2-Windows executables; 
 - `LX` OS/2-ArcaOS standard executables;
 - `PE` Windows NT un/safe applications;
 - `MS-DOS PIF files.

But you sunflower gives a chance to make your own extension of it and
run it with all plugins too.

### Sunflower "seeds" (application plugins)

> [!NOTE]
> Also read the client [guide](WINDOWS.md). It has little-detailed information
> about debugging of plugins

For making new sunflower extension:
1) Create Visual Studio solution.
2) Add reference `SunFlower.Abstractions.dll`
3) Make sure: no differences between Client app version and Abstractions
4) Follow this template
5) Read and learn [versioning](VERSIONING.md).

```csharp
[FlowerContract(4, 0, 0)]
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
[<FlowerContract(4, 0, 0)>]
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

6) Build and Drop .DLL into `%Application%/Plugins`

### Supported Binary Formats

Out-of-box DLLs are in [plugins](https://github.com/AlexeyTolstopyatov/SunFlower.Plugins/) repo

### An architecture problems that seriously bother me, but I can't fix them

 - **Stupid** Exceptions handling -
A `Main` procedure contains exceptions handler which
rewrites Status last error field. Loader prints this message with `-> Disabled plugins tracing` brakets.
 - Versions incompatibility - Unfortunately Sunflower plugins which are differ the foundation are **incompatible** at the moment of updating documentation.
Any differences between foundation file version and plugins foundation calls force exit (means conflict behaviour).

### Frameworks And other external toolchain

Despite the fact that the loader's core uses only the capabilities of `.NET` Core platform, and bundled with the loader's main plugins are written from scratch without the use of external tools, the window application `Sunflower.Windows.exe` uses many different add-ons to be more comfortable and modern. 

All frameworks and toolkits
 - WPF `.net-windows7.0` - Foundation of Windowed client
 - `.NET 8.0` - Foundation of everything
 - [HandyControls](https://github.com/HandyOrg/HandyControl) `3.4.0` - better Window controls / little MVVM experience
 - [Microsoft WPF Behaviours](https://github.com/microsoft/XamlBehaviorsWpf) - The MVVM experience
 - [Monaco](https://github.com/microsoft/monaco-editor) `0.52` - All flower-extension results in one document 
 - [Monaco-Markdown](https://github.com/trofimander/monaco-markdown) - Highlighting extension for Markdown documents
 - Win32 bindings - `OpenFileDialog` / `SaveFileDialog` bindings
 - [Microsoft Web View](https://github.com/MicrosoftEdge/WebView2Browser) - Toolkit for Monaco editor support.
 - [HexView](https://github.com/fjeremic/HexView.Wpf) - Hexadecimal view of file

### Documents and Sources
 - DoDi's VB Decompiler sources (MIT)
 - VBGamer45 - Semi VB Decompiler sources (MIT) 
 - Ghidra part of utils source (Apache-2.0 license)
 - Microsoft NE Segmented EXE format
 - OS/2 OMF (Object Module Format) docs
 - [Microsoft PE Format](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)
 - [Microsoft DOS PIF Web Archive copy](https://web.archive.org/web/20220214185118/http://www.smsoft.ru/en/pifdoc.htm)
 - [Suggesting Visual Basic 4.0 internals](https://gist.github.com/AlexeyTolstopyatov/96a4d36639256fb624e32ae6153bfa11) - SemiVB Decompiler
 - [Suggesting Visual Basic 3.0 internals](https://gist.github.com/AlexeyTolstopyatov/fc19496b8b1a9a5bbb5f12c415c0c1f3) - DoDi VB Decompiler


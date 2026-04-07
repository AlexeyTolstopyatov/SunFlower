# SunFlower kernel API usage

<img src="assets/sunflower.svg" align="right" width="128" height="128">
Assembly `SunFlower.Kernel.dll` uses for plugins data management

### How the client knows about imports | `FlowerBinarySeeker`

The `FlowerBinarySeeker` is an embedded module which determines various program formats.

Usage:
```cs
//  fileInfo -> FlowerFileInfo
var fileInfo = FlowerBinarySeeker.Get("libsmth.so") 
```

Binary seeker uses at the client side in Home page:

![Home](/assets/home_dark.png) 

### Plugins Management | `FlowerSeedManager`

When user selects item into imported files, client calls 
all .NET assemblies in the `./Plugins` directory. Interfaces what implements
`IFlowerSeed` commuication rules, stores `[Flower]` & `[FlowerContract]` metadata
loads into process memory.

Listing 1 shows how to use plugins manager 
```cs
var pluginsList = 
    FlowerSeedManager
        .CreateInstance()
        .LoadAllFlowerSeeds()
        .Seeds
```
Now the plugin instances are loaded in memory but with no results.
To update interfaces call `updateAllFlowerSeeds` method 
Listing 2 shows how to call loaded plugins by knowing target file path 
```cs
var pluginsList = 
    FlowerSeedManager
        .CreateInstance()
        .LoadAllFlowerSeeds()
        .UpdateAllInvokedFlowerSeeds("libsmth.so")
        .Seeds;
```
Summary of loaded plugins could get by `getAllInvokedFlowerSeeds`
Listing 3 shows how to get the plugins work summary
```cs
var pluginsDict = 
    FlowerSeedManager
        .CreateInstance()
        .LoadAllFlowerSeeds()
        .UpdateAllInvokedFlowerSeeds()
        .GetAllInvokedFlowerSeeds();

```
After calling entry point for each loaded plugin interface in the list
function returns `(string, int32)` dictionary where contains exection results.
For example we have three plugins in the list. 
Then expected the table same with following next 

| EntryPoint                | Returns |
|---------------------------|---------|
| `PortableExecutableSeed`  | 0       |
| `NewExecutableSeed`       | 0       |
| `Shell32LinkSeed`         | -1      |

Where `Returns` column having `IFlowerSeed.Main(string)` returning values

All plugins what have no results after applying them on the target file, may contain exceptions (their `LastError` field not null)

If you don't use them, managers fluent API represents
the procedure of unloading

```cs
    FlowerSeedManager
        ...
        .UnloadAllInvokedFlowerSeeds() // *
        .GetAllInvokedFlowerSeeds();

```

Before calling it, in the previous table of loaded plugins
row `Shell32LinkSeed` was removed

| EntryPoint                | Returns |
|---------------------------|---------|
| `PortableExecutableSeed`  | 0       |
| `NewExecutableSeed`       | 0       |

### Compatibility Manager | `FlowerCompatibility`

Same with connecting plugins, the plugin loader (a.k.a. `FlowerSeedsManager`) has `[FlowerContract]` attribute too.
It helps others to define right behavior while assemblies are loading.

Listing 1 shows how to collect data from all installed plugins

```cs
using SunFlower.Kernel.Services

var infoList = FlowerCompatibility.GetFlowerVersionInfoList();
```
Results what you get in the table looks same with given
```json
// Assembiles list
[
    {
        "name": "Sunflower.NE.dll",
        "version": "4.0.0.0",
        // Assembly IFlowerSeed derivate classes list
        "contracts": [
            {
                "name": "NewExecutableFlowerSeed.Seed",  
                "version": {
                    "major": 4,  // \
                    "minor": 0,  //  Taken from [FlowerContract]
                    "build": 0,  // /
                    "private": 0 // <-- Manager Generated. Unused
                }
            },
            {
                "name": "NewExecutableDisasm.Seed",
                "version": {/*...*/}
            }
        ]
    }, 
    /*{ ... }*/
]
```

This versioning characteristics JSON just represents extracted metadata of all plugins. If you want use it for once known plugin,
use `tryGetFlowerVersionInfo`

```cs
var knowingPluginInfo = TryGetFlowerVersionInfo("./Plugins/MyPlugin.dll")
```

> [!WARNING]
> At the moment of publishing documents (07.04.2026), `tryGetFlowerVersionInfo` function
> silently handles exceptions and doesn't return state flag